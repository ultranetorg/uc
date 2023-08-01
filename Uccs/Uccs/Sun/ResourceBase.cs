using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Org.BouncyCastle.Utilities.Encoders;

namespace Uccs.Net
{
	public class ReleaseBaseItem
	{
		public ResourceAddress					Address;
		public byte[]							Hash;
		public Availability						Availability;
		public List<MembersResponse.Member>		DeclaredOn = new();
		public MembersResponse.Member[]			DeclareTo;

		public ReleaseBaseItem(ResourceAddress address, byte[] hash)
		{
			Address = address;
			Hash = hash;
		}
	}
	
	public class ResourceBase
	{
		public const long				PieceMaxLength = 512 * 1024;
		public const string				StatusFile = ".status";

		string							PackagesPath;
		public List<ReleaseBaseItem>	Releases = new();
		public List<FileDownload>			Downloads = new();
		Thread							DeclaringThread;
		public Core						Core;
		public object					Lock = new object();
		public Zone						Zone;

		public ResourceBase(Core core, Zone zone, string packagespath)
		{
			Core = core;
			Zone = zone;

			PackagesPath = packagespath;

			Directory.CreateDirectory(PackagesPath);

			Releases = Directory.EnumerateDirectories(PackagesPath)
									.SelectMany(a => Directory.EnumerateDirectories(a)
										.SelectMany(r => Directory.EnumerateDirectories(r)
											.Select(z => new ReleaseBaseItem(PathToAddress(z), Hex.Decode(Path.GetFileName(z))))))
												.ToList();

			if(core != null && !core.IsClient)
			{
				DeclaringThread = new Thread(Declaring);
				DeclaringThread.Name = $"{Core.Settings.IP.GetAddressBytes()[3]} Declaring";
				DeclaringThread.Start();
			}
		}

		string Escape(string resource)
		{
			return resource.Replace('/', ' ');
		}

		string Unescape(string resource)
		{
			return resource.Replace(' ', '/');
		}

		public void SetLatest(ResourceAddress release, byte[] hash)
		{
			File.WriteAllText(Path.Join(PackagesPath, release.Author, Escape(release.Resource),  "latest"), Hex.ToHexString(hash));
		}

		public string AddressToPath(ResourceAddress resource, byte[] hash)
		{
			string h;

			if(hash == null)
				h = File.ReadAllText(Path.Join(PackagesPath, resource.Author, Escape(resource.Resource),  "latest"));
			else
				h = Hex.ToHexString(hash);

			return Path.Join(PackagesPath, resource.Author, Escape(resource.Resource), h);
		}

		public string AddressToPath(ResourceAddress resource, byte[] hash, string file)
		{
			return Path.Join(AddressToPath(resource, hash), file);
		}

		public ResourceAddress PathToAddress(string path)
		{
			path = path.Substring(PackagesPath.Length).TrimStart(Path.DirectorySeparatorChar);

			var s = path.Split(Path.DirectorySeparatorChar);

			return new ResourceAddress(s[0], Unescape(s[1]));
		}

// 		public string GetDirectory(ResourceAddress release, byte[] hash/*, bool create*/)
// 		{
// 			var p = AddressToPath(release, hash);
// 
// 			//if(create)
// 			//	Directory.CreateDirectory(p);
// 
// 			return p;
// 		}

		public ReleaseBaseItem Add(ResourceAddress resource, byte[] hash)
		{
			var r = new ReleaseBaseItem(resource, hash);
	
			Releases.Add(r);
	
			Directory.CreateDirectory(AddressToPath(resource, hash));

			return r;
		}

		public ReleaseBaseItem Find(ResourceAddress resource, byte[] hash)
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

		public FileDownload DownloadFile(ResourceAddress release, byte[] hash, string file, byte[] filehash, Workflow workflow)
		{
			var d = Downloads.Find(j => j.Resource == release && j.Hash.SequenceEqual(hash) && j.File == file);
				
			if(d == null)
			{
				d = new FileDownload(Core, release, hash, file, filehash, workflow);
				Downloads.Add(d);
			}
		
			return d;
		}

		public void GetFile(ResourceAddress release, byte[] hash, string file, byte[] filehash, Workflow workflow)
		{
			Task t = Task.CompletedTask;

			lock(Lock)
			{
				if(!Exists(release, hash, file))
				{
					var d = DownloadFile(release, hash, file, filehash, workflow);
			
					t = d.Task;
				}
			}

			t.Wait(workflow.Cancellation.Token);
		}

		void Declaring()
		{
			Core.Workflow.Log?.Report(this, "Declaring started");

			var tasks = new Dictionary<AccountAddress, Task>(32);

			try
			{
				while(Core.Workflow.Active)
				{
					Core.Workflow.Wait(100);

					ReleaseBaseItem[] rs;
					//List<Peer> used;

					lock(Lock)
					{
						rs = Releases.Where(i => i.DeclaredOn.Count < 8).ToArray();
						//used = rs.SelectMany(i => i.DeclaredOn).Distinct().Where(h => rs.All(r => r.DeclaredOn.Contains(h))).ToList();
					}

					if(!rs.Any())
						continue;

					var cr = Core.Call<MembersResponse>(Role.Base, i => i.GetMembers(), Core.Workflow);
	
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
									Task.WaitAny(ts, Core.Workflow.Cancellation.Token);
								}
								Monitor.Enter(Lock);
							}

							var drs = Releases.Where(i => i.DeclareTo.Contains(m) && !i.DeclaredOn.Contains(m)).ToArray();

							var t = Task.Run(() =>	{
														Core.Send(m.HubIPs.Random(), p => p.DeclareRelease(drs.Select(i => new DeclareReleaseItem {Hash = i.Hash, Availability = i.Availability}).ToArray()), Core.Workflow);

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
			catch(OperationCanceledException)
			{
			}
			catch(Exception ex) when (!Debugger.IsAttached)
			{
				Core.Stop(MethodBase.GetCurrentMethod(), ex);
			}
		}
	}
}
