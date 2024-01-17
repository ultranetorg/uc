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
	public class ReleaseDeclaration
	{
		public byte[]			Hash { get; set; }
		public Availability		Availability { get; set; }	
	}

	public class ResourceDeclaration
	{
		public ResourceAddress			Resource { get; set; }	
		public ReleaseDeclaration[]		Releases { get; set; }	
	}

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
									.Select(z => new LocalRelease(this, Path.GetFileName(z).FromHex(), DataType.None))
										.ToList();

			if(sun != null && !sun.IsClient)
			{
				DeclaringThread = sun.CreateThread(Declaring);
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

		public LocalRelease Add(byte[] release, DataType type)
		{
			if(Releases.Any(i => i.Hash.SequenceEqual(release)))
				throw new ResourceException(ResourceError.AlreadyExists);

			var r = new LocalRelease(this, release, type);
			r.__StackTrace = new System.Diagnostics.StackTrace(true);
	
			Releases.Add(r);
	
			Directory.CreateDirectory(r.Path);

			return r;
		}

		public LocalResource Add(ResourceAddress resource)
		{
			if(Resources.Any(i => i.Address == resource))
				throw new ResourceException(ResourceError.AlreadyExists);

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
				r = new LocalRelease(this, hash, DataType.None);
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
 				
			var r = Add(h, DataType.Directory);

			r.AddFile(".index", ms.ToArray());

			foreach(var i in files)
			{
				r.AddFile(i.Value, File.ReadAllBytes(i.Key));
			}
			
			r.Complete(Availability.Full);
			
			(Find(resource) ?? Add(resource)).AddData(DataType.Directory, h);

			return r;
		}

		public LocalRelease Build(ResourceAddress resource, string source, Workflow workflow)
		{
			var b = File.ReadAllBytes(source);

			var h = Zone.Cryptography.HashFile(b);
 				
			var r = Add(h, DataType.File);

 			r.AddFile("f", b);
			r.Complete(Availability.Full);
			
			(Find(resource) ?? Add(resource)).AddData(DataType.File, h);

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

			while(Sun.Workflow.Active)
			{
				Sun.Statistics.Declaring.Begin();

				var cr = Sun.Call(i => i.GetMembers(), Sun.Workflow);
	
				if(!cr.Members.Any())
					continue;

				var ds = new Dictionary<MembersResponse.Member, Dictionary<ResourceAddress, List<LocalRelease>>>();

				lock(Lock)
				{
					foreach(var r in Resources.Where(i => i.Last != null))
					{ 
						switch(r.Last.Type)
						{
							case DataType.File:
							case DataType.Directory:
							{
								var lr = Find(r.LastAs<byte[]>());

								if(lr != null && lr.Availability != Availability.None)
								{
									foreach(var m in cr.Members.OrderByNearest(lr.Hash).Take(MembersPerDeclaration).Where(m => !lr.DeclaredOn.Any(dm => dm.Account == m.Account)))
									{
										(ds.TryGetValue(m, out var x) ? x : (ds[m] = new()))[r.Address] = new() {lr};
									}

								}
								break;
							}								
							case DataType.Package:
							{	
								foreach(var lr in (r.LastAs<History>()).Releases.Select(i => Find(i.Hash)).Where(i => i is not null && i.Availability != Availability.None))
								{
									foreach(var m in cr.Members.OrderByNearest(lr.Hash).Take(MembersPerDeclaration).Where(m => !lr.DeclaredOn.Any(dm => dm.Account == m.Account)))
									{
										var a = (ds.TryGetValue(m, out var x) ? x : (ds[m] = new()));
										(a.TryGetValue(r.Address, out var y) ? y : (a[r.Address] = new())).Add(lr);
									}
								}

								break;
							}
						}
					}
				}

				if(!ds.Any())
				{
					Sun.Statistics.Declaring.End();
					Thread.Sleep(1000);
					continue;
				}

				lock(Lock)
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

					foreach(var i in ds)
					{
						var t = Task.Run(() =>	{
													DeclareReleaseResponse drr;

													try
													{
														drr = Sun.Call(i.Key.SeedHubRdcIPs.Random(), p => p.Request<DeclareReleaseResponse>(new DeclareReleaseRequest {Resources = i.Value.Select(rs => new ResourceDeclaration{	Resource = rs.Key, 
																																																									Releases = rs.Value.Select(rl => new ReleaseDeclaration{Hash = rl.Hash, 
																																																																							Availability  = rl.Availability}).ToArray() }).ToArray() }), Sun.Workflow);
													}
													catch(NodeException)
													{
														return;
													}
	
													lock(Lock)
													{
														foreach(var r in drr.Results)
															if(r.Result == DeclarationResult.Accepted)
																Find(r.Hash).DeclaredOn.Add(i.Key);

														tasks.Remove(i.Key.Account);
													}
												});
						tasks[i.Key.Account] = t;
					}
				}
					
				Sun.Statistics.Declaring.End();
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

		public ReleaseDownloadProgress GetDownloadProgress(LocalRelease release)
		{
			var f = FileDownloads.Find(i => i.Release == release);
			var d = DirectoryDownloads.Find(i => i.Release == release);

			if(d != null || f != null)
			{
				var s = new ReleaseDownloadProgress(d?.SeedCollector ?? f?.SeedCollector);

				s.Succeeded	= d != null ? d.Succeeded : f.Succeeded;

				s.CurrentFiles = FileDownloads.Where(i => i.Release == (d?.Release ?? f?.Release)).Select(i => new FileDownloadProgress(i)).ToArray();

				return s;
			}
			
			return null;
		}
	}
}
