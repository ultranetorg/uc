using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RocksDbSharp;

namespace Uccs.Net
{
	public class ResourceHub
	{
		public const long				PieceMaxLength = 512 * 1024;
		public const string				ReleaseFamilyName = "Releases";
		public const string				ResourceFamilyName = "Resources";
		public const int				MembersPerDeclaration = 3;

		internal string					ReleasesPath;
		public List<LocalRelease>		Releases = new();
		public List<LocalResource>		Resources = new();
		public List<FileDownload>		FileDownloads = new();
		public List<DirectoryDownload>	DirectoryDownloads = new();
		public Sun						Sun;
		public object					Lock = new object();
		public Zone						Zone;
		public ColumnFamilyHandle		ReleaseFamily => Sun.Database.GetColumnFamily(ReleaseFamilyName);
		public ColumnFamilyHandle		ResourceFamily => Sun.Database.GetColumnFamily(ResourceFamilyName);
		Thread							DeclaringThread;

		public ResourceHub(Sun sun, Zone zone, string path)
		{
			Sun = sun;
			Zone = zone;

			ReleasesPath = path;

			Directory.CreateDirectory(ReleasesPath);

			Releases = Directory.EnumerateDirectories(ReleasesPath)
									.Select(z => new LocalRelease(this, Path.GetFileName(z).FromHex(), ResourceType.None))
										.ToList();

			if(sun != null && !sun.IsClient)
			{
				DeclaringThread = new Thread(Declaring);
				DeclaringThread.Name = $"{Sun.Settings.IP.GetAddressBytes()[3]} Declaring";
				DeclaringThread.Start();
			}
		}

		public string Escape(string resource)
		{
			return resource.Replace('/', ' ');
		}

		public string Unescape(string resource)
		{
			return resource.Replace(' ', '/');
		}

		//public void SetLatest(ResourceAddress release, byte[] hash)
		//{
		//	File.WriteAllText(Path.Join(ReleasesPath, release.Author, Escape(release.Resource),  "latest"), hash.ToHex());
		//}
		//
		//public byte[] GetLatest(ResourceAddress release)
		//{
		//	var f = Path.Join(ReleasesPath, release.Author, Escape(release.Resource),  "latest");
		//	return File.Exists(f) ? File.ReadAllText(f).FromHex() : null;
		//}

		//public ReleaseAddress PathToAddress(string path)
		//{
		//	path = path.Substring(ReleasesPath.Length).TrimStart(Path.DirectorySeparatorChar);
		//
		//	var s = path.Split(Path.DirectorySeparatorChar);
		//
		//	return new ReleaseAddress(s[0], Unescape(s[1]), s[2].FromHex());
		//}

		public LocalRelease Add(byte[] release, ResourceType type)
		{
			if(Releases.Any(i => i.Hash.SequenceEqual(release)))
				throw new ResourceException($"Release {release.ToHex()} already exists");

			var r = new LocalRelease(this, release, type);
	
			Releases.Add(r);
	
			Directory.CreateDirectory(r.Path);

			return r;
		}

		public LocalResource Add(ResourceAddress resource)
		{
			if(Resources.Any(i => i.Address == resource))
				throw new ResourceException($"Resource {resource} already exists");

			var r = new LocalResource(this, resource) {Datas = new()};
	
			Resources.Add(r);

			r.Save();

			return r;
		}

		public LocalRelease Find(byte[] hash)
		{
			var r = Releases.Find(i => i.Hash.SequenceEqual(hash));

			if(r != null)
				return r;

			var d = Sun.Database.Get(hash, ReleaseFamily);

			if(d != null)
			{
				r = new LocalRelease(this, hash, ResourceType.None);
				Releases.Add(r);
				return r;
			}

			return null;
		}

		public LocalResource Find(ResourceAddress resource)
		{
			var r = Resources.Find(i => i.Address == resource);

			if(r != null)
				return r;

			var d = Sun.Database.Get(Encoding.UTF8.GetBytes(resource.ToString()), ResourceFamily);

			if(d != null)
			{
				r = new LocalResource(this, resource);
				r.Load();
				Resources.Add(r);
				return r;
			}

			return null;
		}

// 		public LocalRelease Find(ReleaseAddress release)
// 		{
// 			if(release.Hash != null)
// 				return Find(release.Hash);
// 
// 			var r = Find(release.Resource);
// 
// 			if(r != null)
// 				return Find(r.Last);
// 
// 			return null;
// 		}

		public LocalRelease Build(ResourceAddress resource, IEnumerable<string> sources, Workflow workflow)
		{
			var files = new Dictionary<string, string>();
			var index = new XonDocument(new XonBinaryValueSerializator());

			void adddir(string basepath, Xon parent, string dir, string dest)
			{
				//var d = parent.Add(Path.GetFileName(path));

				foreach(var i in Directory.EnumerateFiles(dir))
				{
					files[i] = Path.Join(dest, i.Substring(basepath.Length + 1).Replace(Path.DirectorySeparatorChar, '/'));
					parent.Add(Path.GetFileName(i)).Value = Zone.Cryptography.HashFile(File.ReadAllBytes(i));
				}

				foreach(var i in Directory.EnumerateDirectories(dir).Where(i => Directory.EnumerateFileSystemEntries(i).Any()))
				{
					var d = parent.Add(Path.GetFileName(i));

					adddir(basepath, d, i, dest);
				}
			}

			foreach(var i in sources)
			{
				var sd = i.Split('=');
				var s = sd[0];
				var d = sd.Length == 2 ? sd[1] : null;

				if(d == null)
				{
					if(Directory.Exists(s))
					{
						adddir(s, index, s, null);
					}
					else
					{
						files[s] = Path.GetFileName(s);
						index.Add(files[s]).Value = Zone.Cryptography.HashFile(File.ReadAllBytes(s));
					}
				}
				else
				{
					if(Directory.Exists(s))
					{
						adddir(s, index, s, d);
					}
					else
					{
						files[s] = d;
						index.Add(files[s]).Value = Zone.Cryptography.HashFile(File.ReadAllBytes(s));
					}
				}
			}

			var ms = new MemoryStream();
			index.Save(new XonBinaryWriter(ms));

			var h = Zone.Cryptography.HashFile(ms.ToArray());
 				
			var r = Add(h, ResourceType.Directory);

			r.AddFile(".index", ms.ToArray());

			foreach(var i in files)
			{
				r.AddFile(i.Value, File.ReadAllBytes(i.Key));
			}
			
			r.Complete(Availability.Full);
			
			(Find(resource) ?? Add(resource)).AddData(h);

			return r;
		}

		public LocalRelease Build(ResourceAddress resource, string source, Workflow workflow)
		{
			var b = File.ReadAllBytes(source);

			var h = Zone.Cryptography.HashFile(b);
 				
			var r = Add(h, ResourceType.File);

 			r.AddFile("f", b);
			
			r.Complete(Availability.Full);
			
			(Find(resource) ?? Add(resource)).AddData(h);

			return r;
		}

		public void GetFile(LocalRelease release, string file, byte[] filehash, SeedCollector peerCollector, Workflow workflow)
		{
			Task t = Task.CompletedTask;

			lock(Lock)
			{
				if(!release.IsReady(file))
				{
					var d = DownloadFile(release, file, filehash, peerCollector, workflow);
			
					t = d.Task;
				}
			}

			t.Wait(workflow.Cancellation);
		}

		void Declaring()
		{
			Sun.Workflow.Log?.Report(this, "Declaring started");

			var tasks = new Dictionary<AccountAddress, Task>(32);

			try
			{
				while(Sun.Workflow.Active)
				{
					Sun.Workflow.Wait(100);

					LocalRelease[] rs;
					//List<Peer> used;

					lock(Lock)
					{
						rs = Releases.Where(i => i.Availability != Availability.None && i.DeclaredOn.Count < MembersPerDeclaration).ToArray();
						//used = rs.SelectMany(i => i.DeclaredOn).Distinct().Where(h => rs.All(r => r.DeclaredOn.Contains(h))).ToList();
					}

					if(!rs.Any())
						continue;

					var cr = Sun.Call(i => i.GetMembers(), Sun.Workflow);
	
					if(!cr.Members.Any())
						continue;

					lock(Lock)
					{
						foreach(var r in Releases)
						{
							r.DeclareTo = cr.Members.OrderByNearest(r.Hash).Take(MembersPerDeclaration).ToArray();
						}

						foreach(var m in cr.Members.Where(i => i.SeedHubRdcIPs.Any()))
						{
							if(tasks.Count >= 32)
							{
								var ts = tasks.Select(i => i.Value).ToArray();

								Monitor.Exit(Lock);
								{
									Task.WaitAny(ts, Sun.Workflow.Cancellation);
								}
								Monitor.Enter(Lock);
							}

							var drs = Releases.Where(i => i.DeclareTo != null && i.DeclareTo.Any(i => i.Account == m.Account) && !i.DeclaredOn.Any(i => i.Account == m.Account)).ToArray();

							if(drs.Any())
							{
								var t = Task.Run(() =>	{
															try
															{
																Sun.Send(m.SeedHubRdcIPs.Random(), p => p.DeclareRelease(drs.Select(i => new DeclareReleaseItem{Hash = i.Hash, Availability  = i.Availability}).ToArray()), Sun.Workflow);
															}
															catch(RdcNodeException)
															{
															}
	
															lock(Lock)
															{
																foreach(var i in drs)
																	i.DeclaredOn.Add(m);

																tasks.Remove(m.Account);
															}
														});
								tasks[m.Account] = t;
							}
						}
					}
				}
			}
			catch(OperationCanceledException)
			{
			}
			catch(Exception ex) when (!Debugger.IsAttached)
			{
				Sun.Stop(MethodBase.GetCurrentMethod(), ex);
			}
		}

		public FileDownload DownloadFile(LocalRelease release, string file, byte[] filehash, SeedCollector peercollector, Workflow workflow)
		{
			var d = FileDownloads.Find(j => j.Release == release && j.File.Path == file);
				
			if(d != null)
				return d;

			d = new FileDownload(Sun, release, file, filehash, peercollector, workflow);
			FileDownloads.Add(d);
		
			return d;
		}

		public DirectoryDownload DownloadDirectory(LocalRelease release, Workflow workflow)
		{
			var d = DirectoryDownloads.Find(j => j.Release == release);
				
			if(d != null)
				return d;
				
			d = new DirectoryDownload(Sun, release, workflow);
			DirectoryDownloads.Add(d);

			return d;
		}

		public ResourceDownloadProgress GetDownloadProgress(LocalRelease release)
		{
			var f = FileDownloads.Find(i => i.Release == release);
			var d = DirectoryDownloads.Find(i => i.Release == release);

			if(d != null || f != null)
			{
				var s = new ResourceDownloadProgress(d?.SeedCollector ?? f?.SeedCollector);

				s.Succeeded	= d != null ? d.Succeeded : f.Succeeded;

				s.CurrentFiles = FileDownloads.Where(i => i.Release == (d?.Release ?? f?.Release)).Select(i => new FileDownloadProgress(i)).ToArray();

				return s;
			}
			
			return null;
		}
	}
}
