using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

		public List<LocalRelease>		Releases = new();
		public List<LocalResource>		Resources = new();
		public Rdn						Rdn;
		public object					Lock = new object();
		public Zone						Zone;
		public ColumnFamilyHandle		ReleaseFamily => Rdn.Database.GetColumnFamily(ReleaseFamilyName);
		public ColumnFamilyHandle		ResourceFamily => Rdn.Database.GetColumnFamily(ResourceFamilyName);
		Thread							DeclaringThread;
		SeedSettings					Settings;

		public ResourceHub(Rdn rds, Zone zone, SeedSettings settings)
		{
			Rdn = rds;
			Zone = zone;
			Settings = settings;

			Directory.CreateDirectory(Settings.Releases);

			using(var i = Rdn.Database.NewIterator(ReleaseFamily))
			{
				for(i.SeekToFirst(); i.Valid(); i.Next())
				{
	 				Releases.Add(new LocalRelease(this, Urr.FromRaw(i.Key())));
				}
			}

			if(rds.Node.IsListener)
			{
				DeclaringThread = rds.Node.CreateThread(Declaring);
				DeclaringThread.Name = $"{Rdn.Node.Settings.IP.GetAddressBytes()[3]} Declaring";
				DeclaringThread.Start();
			}
		}

		public string ToReleases(Urr urr)
		{
			return Path.Join(Settings.Releases, Escape(urr.ToString()));
		}

		public static string Escape(string path)
		{
			return new char[] {' '}.Concat(Path.GetInvalidFileNameChars()).Aggregate(path.ToString(), (c1, c2) => c1.Replace(c2.ToString(), $" {(short)c2} "));
		}

		public static string Unescape(string path)
		{
			return new char[] {' '}.Concat(Path.GetInvalidFileNameChars()).Aggregate(path, (c1, c2) => c1.Replace($" {(short)c2} ", c2.ToString()));
		}

		public LocalRelease Add(byte[] address, DataType type)
		{
			if(Releases.Any(i => i.Address.Raw.SequenceEqual(address)))
				throw new ResourceException(ResourceError.AlreadyExists);
		
			var r = new LocalRelease(this, Urr.FromRaw(address), type);
			r.__StackTrace = new System.Diagnostics.StackTrace(true);
		
			Releases.Add(r);
		
			return r;
		}

		public LocalRelease Add(Urr address, DataType type)
		{
			if(Releases.Any(i => i.Address == address))
				throw new ResourceException(ResourceError.AlreadyExists);

			var r = new LocalRelease(this, address, type);
			r.__StackTrace = new System.Diagnostics.StackTrace(true);
	
			Releases.Add(r);

			return r;
		}

		public LocalResource Add(Ura resource)
		{
			if(Resources.Any(i => i.Address == resource))
				throw new ResourceException(ResourceError.AlreadyExists);

			var r = new LocalResource(this, resource) {Datas = new()};
	
			Resources.Add(r);

			r.Save();

			return r;
		}

//		public LocalRelease Find(byte[] address)
//		{
//			var r = Releases.Find(i => i.Address.Raw.SequenceEqual(address));
//
//			if(r != null)
//				return r;
//
//			var d = Sun.Database.Get(address, ReleaseFamily);
//
//			if(d != null)
//			{
//				r = new LocalRelease(this, ReleaseAddress.FromRaw(address), DataType.None);
//				Releases.Add(r);
//				return r;
//			}
//
//			return null;
//		}

		public LocalRelease Find(Urr address)
		{
			var r = Releases.Find(i => i.Address == address);

			if(r != null)
				return r;

			var d = Rdn.Database.Get(address.Raw, ReleaseFamily);

			if(d != null)
			{
				r = new LocalRelease(this, address, DataType.None);
				Releases.Add(r);
				return r;
			}

			return null;
		}

		public LocalResource Find(Ura resource)
		{
			var r = Resources.Find(i => i.Address == resource);

			if(r != null)
				return r;

			var d = Rdn.Database.Get(Encoding.UTF8.GetBytes(resource.ToString()), ResourceFamily);

			if(d != null)
			{
				r = new LocalResource(this, resource);
				r.Load();
				Resources.Add(r);
				return r;
			}

			return null;
		}

		public LocalRelease Add(IEnumerable<string> sources, ReleaseAddressCreator address, Flow workflow)
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
			var a = address.Create(Rdn, h);
 				
			var r = Add(a, DataType.Directory);

			r.AddCompleted(LocalRelease.Index, null, ms.ToArray());

			foreach(var i in files)
			{
				r.AddExisting(i.Value, i.Key);
			}
			
			r.Complete(Availability.Full);

			return r;
		}

		public LocalRelease Add(string path, ReleaseAddressCreator address, Flow workflow)
		{
			var b = File.ReadAllBytes(path);

			var h = Zone.Cryptography.HashFile(b);
			var a = address.Create(Rdn, h);
 			
			var r = Add(a, DataType.File);

 			r.AddExisting("", path);
			r.Complete(Availability.Full);

			return r;
		}

		public LocalFile GetFile(LocalRelease release, string file, string localpath, IIntegrity integrity, Harvester harvester, Flow workflow)
		{
			var t = Task.CompletedTask;

			lock(Lock)
			{
				if(!release.IsReady(file))
				{
					var d = DownloadFile(release, file, localpath, integrity, harvester, workflow);
			
					t = d.Task;
				}
			}

			t.Wait(workflow.Cancellation);

			return release.Find(file);
		}

		void Declaring()
		{
			Rdn.Flow.Log?.Report(this, "Declaring started");

			var tasks = new Dictionary<AccountAddress, Task>(32);

			while(Rdn.Flow.Active)
			{
				Rdn.Node.Statistics.Declaring.Begin();

				var cr = Rdn.Call(() => new MembersRequest(), Rdn.Flow);
	
				if(!cr.Members.Any())
					continue;

				var ds = new Dictionary<MembersResponse.Member, Dictionary<ResourceId, LocalRelease>>();

				lock(Lock)
				{
					foreach(var r in Resources.Where(i => i.Id != null && i.Last?.Interpretation is Urr))
					{	
						//foreach(var d in r.Datas)
						var d = r.Last;

						switch(d.Type)
						{
							case DataType.File:
							case DataType.Directory:
							case DataType.Package:
							{
								var l = Find(d.Interpretation as Urr);

								if(l != null && l.Availability != Availability.None)
								{
									foreach(var m in cr.Members.OrderByNearest(l.Address.MemberOrderKey).Take(MembersPerDeclaration).Where(m =>	{
																																					var d = l.DeclaredOn.Find(dm => dm.Member.Account == m.Account);
																																					return d == null || d.Status == DeclarationStatus.Failed && DateTime.UtcNow - d.Failed > TimeSpan.FromSeconds(3);
																																				}))
									{
										var rss = ds.TryGetValue(m, out var x) ? x : (ds[m] = new());
										rss[r.Id] = l;
									}

								}
								break;
							}								
							//{	
							//	foreach(var lr in r.Datas.Select(i => Find(i.Data)).Where(i => i is not null && i.Availability != Availability.None))
							//	{
							//		foreach(var m in cr.Members.OrderByNearest(lr.Address).Take(MembersPerDeclaration).Where(m => !lr.DeclaredOn.Any(dm => dm.Account == m.Account)))
							//		{
							//			var a = (ds.TryGetValue(m, out var x) ? x : (ds[m] = new()));
							//			(a.TryGetValue(r.Address, out var y) ? y : (a[r.Address] = new())).Add(lr);
							//		}
							//	}
							//
							//	break;
							//}
						}
					}
				}

				if(!ds.Any())
				{
					Rdn.Node.Statistics.Declaring.End();
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
							Task.WaitAny(ts, Rdn.Flow.Cancellation);
						}
						Monitor.Enter(Lock);
					}

					foreach(var i in ds)
					{
						foreach(var r in i.Value.Select(i => i.Value))
						{
							var d = r.DeclaredOn.Find(j => j.Member.Account == i.Key.Account);

							if(d == null)
								r.DeclaredOn.Add(new Declaration {Member = i.Key, Status = DeclarationStatus.InProgress});
							else
								d.Status = DeclarationStatus.InProgress;
						}

						var t = Task.Run(() =>	{
													DeclareReleaseResponse drr;

													try
													{
														drr = Rdn.Call(i.Key.SeedHubRdcIPs.Random(), () => new DeclareReleaseRequest {Resources = i.Value.Select(rs => new ResourceDeclaration{	Resource = rs.Key, 
																																																Release = rs.Value.Address, 
																																																Availability = rs.Value.Availability }).ToArray()}, Rdn.Flow);
													}
													catch(OperationCanceledException)
													{
														return;
													}
													catch(NodeException)
													{
														return;
													}
	
													lock(Lock)
													{
														foreach(var r in drr.Results)
														{	
															var x = Find(r.Address).DeclaredOn.Find(j => j.Member.Account == i.Key.Account);

															if(r.Result == DeclarationResult.Accepted)
																x.Status = DeclarationStatus.Accepted;
															else if(r.Result == DeclarationResult.Rejected)
															{	
																x.Status = DeclarationStatus.Failed;
																x.Failed = DateTime.UtcNow;
															}
															else
																Find(r.Address).DeclaredOn.Remove(x);
														}

														tasks.Remove(i.Key.Account);
													}
												});
						tasks[i.Key.Account] = t;
					}
				}
					
				Rdn.Node.Statistics.Declaring.End();
			}
		}

		public FileDownload DownloadFile(LocalRelease release, string path, string localpath, IIntegrity integrity, Harvester collector, Flow workflow)
		{
			var f = release.Files.Find(i => i.Path == path);
			
			if(f != null)
			{
				if(f.Activity is FileDownload d0)
					return d0;
				else if(f.Activity != null)
					throw new ResourceException(ResourceError.Busy);
			}

			var d = new FileDownload(Rdn, release, path, localpath, integrity, collector, workflow);
		
			return d;
		}

		public DirectoryDownload DownloadDirectory(LocalRelease release, string localpath, IIntegrity integrity, Flow workflow)
		{
			if(release.Activity is DirectoryDownload d)
				return d;
			else if(release.Activity != null)
				throw new ResourceException(ResourceError.Busy);
				
			d = new DirectoryDownload(Rdn, release, localpath, integrity, workflow);

			return d;
		}
	}
}
