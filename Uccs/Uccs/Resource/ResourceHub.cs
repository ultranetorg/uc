using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using Org.BouncyCastle.Utilities.Encoders;
using RocksDbSharp;

namespace Uccs.Net
{
	public class ResourceHub
	{
		public const long				PieceMaxLength = 512 * 1024;
		public const string				FamilyName = "Resources";

		internal string					ResourcesPath;
		public List<Release>			Releases = new();
		public List<FileDownload>		FileDownloads = new();
		public List<DirectoryDownload>	DirectoryDownloads = new();
		Thread							DeclaringThread;
		public Sun						Sun;
		public object					Lock = new object();
		public Zone						Zone;
		public ColumnFamilyHandle		Family => Sun.Database.GetColumnFamily(FamilyName);

		public ResourceHub(Sun sun, Zone zone, string path)
		{
			Sun = sun;
			Zone = zone;

			ResourcesPath = path;

			Directory.CreateDirectory(ResourcesPath);

			Releases = Directory.EnumerateDirectories(ResourcesPath)
									.SelectMany(a => Directory.EnumerateDirectories(a)
										.SelectMany(r => Directory.EnumerateDirectories(r)
											.Select(z => new Release(this, PathToAddress(z), Hex.Decode(Path.GetFileName(z))))))
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

		public void SetLatest(ResourceAddress release, byte[] hash)
		{
			File.WriteAllText(Path.Join(ResourcesPath, release.Author, Escape(release.Resource),  "latest"), hash.ToHex());
		}

		public string AddressToPath(ResourceAddress resource, byte[] hash)
		{
			string h;

			if(hash == null)
				h = File.ReadAllText(Path.Join(ResourcesPath, resource.Author, Escape(resource.Resource),  "latest"));
			else
				h = Hex.ToHexString(hash);

			return Path.Join(ResourcesPath, resource.Author, Escape(resource.Resource), h);
		}

		public string AddressToPath(ResourceAddress resource, byte[] hash, string file)
		{
			return Path.Join(AddressToPath(resource, hash), file);
		}

		public ResourceAddress PathToAddress(string path)
		{
			path = path.Substring(ResourcesPath.Length).TrimStart(Path.DirectorySeparatorChar);

			var s = path.Split(Path.DirectorySeparatorChar);

			return new ResourceAddress(s[0], Unescape(s[1]));
		}

		public Release Add(ResourceAddress resource, byte[] hash)
		{
			var r = new Release(this, resource, hash);
	
			Releases.Add(r);
	
			Directory.CreateDirectory(AddressToPath(resource, hash));

			return r;
		}

		public Release Add(ResourceAddress resource, IEnumerable<string> sources, Workflow workflow)
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
 				
			var r = Add(resource, h);

 			WriteFile(resource, h, ".index", 0, ms.ToArray());

			foreach(var i in files)
			{
				WriteFile(resource, h, i.Value, 0, File.ReadAllBytes(i.Key));
			}
			
			SetLatest(resource, h);

			return r;
		}

		public Release Add(ResourceAddress resource, string source, Workflow workflow)
		{
			var b = File.ReadAllBytes(source);

			var h = Zone.Cryptography.HashFile(b);
 				
			var r = Add(resource, h);

 			WriteFile(resource, h, "f", 0, b);
			
			SetLatest(resource, h);

			return r;
		}

		public Release Find(ResourceAddress resource, byte[] hash)
		{
			return Releases.Find(i => i.Address == resource && (hash == null || i.Hash.SequenceEqual(hash)));

			//if(r != null)
			//	return r;

			//if(!Directory.Exists(Path.Join(PackagesPath, resource.Author, Escape(resource.Resource))))
			//	return null;
			//
			//if(Directory.Exists(GetDirectory(resource, hash, false)))
			//{
			//	r = new ReleaseBaseItem(resource, hash);
			//
			//	Releases.Add(r);
			//
			//	return r;
			//}

			//return null;
		}

		public byte[] ReadFile(ResourceAddress release, byte[] hash, string file, long offset, long length)
		{
			using(var s = new FileStream(AddressToPath(release, hash, file), FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				s.Seek(offset, SeekOrigin.Begin);
				
				var b = new byte[Math.Min(length, PieceMaxLength)];
	
				s.Read(b);
	
				return b;
			}
		}

		public void WriteFile(ResourceAddress release, byte[] hash, string file, long offset, byte[] data)
		{
			var d = Path.GetDirectoryName(file);
		
			if(d.Any())
			{
				Directory.CreateDirectory(AddressToPath(release, hash, d));
			}

			using(var s = new FileStream(AddressToPath(release, hash, file), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
			{
				s.Seek(offset, SeekOrigin.Begin);
				s.Write(data);
			}
		}

		public bool Exists(ResourceAddress release, byte[] hash, string file)
		{
			return File.Exists(AddressToPath(release, hash, file));
		}

		public byte[] ReadFile(ResourceAddress release, byte[] hash, string file)
		{
			return File.ReadAllBytes(AddressToPath(release, hash, file));
		}

		public byte[] Hashify(ResourceAddress release, byte[] hash, string path)
		{
			return Zone.Cryptography.HashFile(File.ReadAllBytes(AddressToPath(release, hash, path)));
		}

		public long GetLength(ResourceAddress release, byte[] hash, string path)
		{
			return Exists(release, hash, path) ? new FileInfo(AddressToPath(release, hash, path)).Length : 0;
		}

		public void GetFile(Release release, string file, byte[] filehash, SeedCollector peerCollector, Workflow workflow)
		{
			Task t = Task.CompletedTask;

			lock(Lock)
			{
				if(!Exists(release.Address, release.Hash, file))
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

					Release[] rs;
					//List<Peer> used;

					lock(Lock)
					{
						rs = Releases.Where(i => i.DeclaredOn.Count < 8).ToArray();
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
							r.DeclareTo = cr.Members.OrderBy(i => BigInteger.Abs(new BigInteger(i.Account) - new BigInteger(new Span<byte>(r.Hash, 0, 20)))).Take(8).ToArray();
						}

						foreach(var m in cr.Members.Where(i => i.HubIPs.Any()))
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

							var drs = Releases.Where(i => i.DeclareTo != null && i.DeclareTo.Contains(m) && !i.DeclaredOn.Contains(m)).Select(i => new {r = i, h = i.Hash, a = i.Availability}).ToArray();

							var t = Task.Run(() =>	{
														try
														{
															Sun.Send(m.HubIPs.Random(), p => p.DeclareRelease(drs.Select(i => new DeclareReleaseItem {	Hash = i.h, 
																																						Availability  = i.a
																																					}).ToArray()), Sun.Workflow);
														}
														catch(ConnectionFailedException)
														{
														}

														lock(Lock)
														{
															foreach(var i in drs)
																i.r.DeclaredOn.Add(m);
												
															tasks.Remove(m.Account);
														}
													});
							tasks[m.Account] = t;
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

		public FileDownload DownloadFile(Release release, string file, byte[] filehash, SeedCollector peercollector, Workflow workflow)
		{
			var d = FileDownloads.Find(j => j.Release == release && j.File.Path == file);
				
			if(d != null)
				return d;

			d = new FileDownload(Sun, release, file, filehash, peercollector, workflow);
			FileDownloads.Add(d);
		
			return d;
		}

		public DirectoryDownload DownloadDirectory(Release release, Workflow workflow)
		{
			var d = DirectoryDownloads.Find(j => j.Release == release);
				
			if(d != null)
				return d;
				
			d = new DirectoryDownload(Sun, release, workflow);
			DirectoryDownloads.Add(d);

			return d;
		}
	}
}
