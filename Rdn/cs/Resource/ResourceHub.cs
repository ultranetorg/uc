﻿using System.Text;
using RocksDbSharp;

namespace Uccs.Rdn;

public class ResourceHub
{
	public const long				PieceMaxLength = 512 * 1024;
	public const string				ReleaseFamilyName = "Releases";
	public const string				ResourceFamilyName = "Resources";
	public const int				MembersPerDeclaration = 3;

	public List<LocalRelease>		Releases = new();
	public List<LocalResource>		Resources = new();
	public RdnNode					Node;
	public object					Lock = new object();
	public McvNet					Net;
	public ColumnFamilyHandle		ReleaseFamily => Node.Database.GetColumnFamily(ReleaseFamilyName);
	public ColumnFamilyHandle		ResourceFamily => Node.Database.GetColumnFamily(ResourceFamilyName);
	public SeedSettings				Settings;
	Thread							DeclaringThread;

	public ResourceHub(RdnNode node, McvNet net, SeedSettings settings)
	{
		Node = node;
		Net = net;
		Settings = settings;

		Settings.Releases ??= Path.Join(Node.Settings.Profile, nameof(Settings.Releases));

		Directory.CreateDirectory(Settings.Releases);

		if(!Node.Database.TryGetColumnFamily(ReleaseFamilyName, out var cf))	Node.Database.CreateColumnFamily(new (), ReleaseFamilyName);
		if(!Node.Database.TryGetColumnFamily(ResourceFamilyName, out cf))		Node.Database.CreateColumnFamily(new (), ResourceFamilyName);

		using(var i = Node.Database.NewIterator(ReleaseFamily))
		{
			for(i.SeekToFirst(); i.Valid(); i.Next())
			{
 				Releases.Add(new LocalRelease(this, Urr.FromRaw(i.Key())));
			}
		}

	}

	public void RunDeclaring()
	{ 
		if(Node.Peering.IsListener)
		{
			DeclaringThread = Node.CreateThread(Declaring);
			DeclaringThread.Name = $"{Node.Name} Declaring";
			DeclaringThread.Start();
		}
	}

	public string ToReleases(Urr urr)
	{
		return Path.Join(Settings.Releases, Uccs.Net.Net.Escape(urr.ToString()));
	}

	public LocalRelease Add(byte[] address)
	{
		if(Releases.Any(i => i.Address.Raw.SequenceEqual(address)))
			throw new ResourceException(ResourceError.AlreadyExists);
	
		var r = new LocalRelease(this, Urr.FromRaw(address));
		r.__StackTrace = new System.Diagnostics.StackTrace(true);
	
		Releases.Add(r);
	
		return r;
	}

	public LocalRelease Add(Urr address)
	{
		if(Releases.Any(i => i.Address == address))
			throw new ResourceException(ResourceError.AlreadyExists);

		var r = new LocalRelease(this, address);
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

		var d = Node.Database.Get(address.Raw, ReleaseFamily);

		if(d != null)
		{
			r = new LocalRelease(this, address);
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

		var d = Node.Database.Get(Encoding.UTF8.GetBytes(resource.ToString()), ResourceFamily);

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
		var index = new Xon(new XonBinaryValueSerializator());

		void adddir(string basepath, Xon parent, string dir, string dest)
		{
			//var d = parent.Add(Path.GetFileName(path));

			foreach(var i in Directory.EnumerateFiles(dir))
			{
				files[i] = Path.Join(dest, i.Substring(basepath.Length + 1).Replace(Path.DirectorySeparatorChar, '/'));
				parent.Add(Path.GetFileName(i)).Value = Net.Cryptography.HashFile(File.ReadAllBytes(i));
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
					index.Add(files[s]).Value = Net.Cryptography.HashFile(File.ReadAllBytes(s));
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
					index.Add(files[s]).Value = Net.Cryptography.HashFile(File.ReadAllBytes(s));
				}
			}
		}

		var ms = new MemoryStream();
		index.Save(new XonBinaryWriter(ms));

		var h = Net.Cryptography.HashFile(ms.ToArray());
		var a = address.Create(null/*Node.Vault*/, h);
 				
		var r = Add(a);

		r.AddCompleted(LocalRelease.Index, null, ms.ToArray());

		foreach(var i in files)
		{
			r.AddCompleted(i.Value, i.Key, null);
		}
		
		r.Complete(Availability.Full);

		return r;
	}

	public LocalRelease Add(string path, ReleaseAddressCreator address, Flow workflow)
	{
		var b = File.ReadAllBytes(path);

		var h = Net.Cryptography.HashFile(b);
		var a = address.Create(null/*Node.Vault*/, h);
 			
		var r = Add(a);

 			r.AddCompleted("", path, null);
		r.Complete(Availability.Full);

		return r;
	}

	public LocalFile GetFile(LocalRelease release, bool single, string file, string localpath, IIntegrity integrity, SeedSeeker harvester, Flow workflow)
	{
		var t = Task.CompletedTask;

		lock(Lock)
		{
			if(!release.IsReady(file))
			{
				var d = DownloadFile(release, single, file, localpath, integrity, harvester, workflow);
		
				t = d.Task;
			}
		}

		t.Wait(workflow.Cancellation);

		return release.Find(file);
	}

	void Declaring()
	{
		Node.Flow.Log?.Report(this, "Declaring started");

		var tasks = new Dictionary<object, Task>(32);

		while(Node.Flow.Active)
		{
			Node.Peering.Statistics.Declaring.Begin();

			var cr = Node.Peering.Call(() => new RdnMembersRequest(), Node.Flow);

			if(!cr.Members.Any())
				continue;

			var ds = new Dictionary<RdnGenerator, Dictionary<LocalResource, LocalRelease>>();
			var us = new List<LocalResource>();

			lock(Lock)
			{
				foreach(var r in Resources.Where(i => i.Last?.Type.Control == DataType.File || i.Last?.Type.Control == DataType.Directory))
				{
					if(r.Id == null)
					{
						us.Add(r);
					} 
					else
					{
						//foreach(var d in r.Datas)
						var d = r.Last;

						var l = Find(d.Parse<Urr>());

						if(l != null && l.Availability != Availability.None)
						{
							foreach(var m in cr.Members	.OrderByHash(i => i.Address.Bytes, l.Address.MemberOrderKey)
														.Take(MembersPerDeclaration)
														.Where(m =>	{
																		var d = l.DeclaredOn.Find(dm => dm.Member.Address == m.Address);
																		return d == null || d.Status == DeclarationStatus.Failed && DateTime.UtcNow - d.Failed > TimeSpan.FromSeconds(3);
																	})
														.Cast<RdnGenerator>())
							{
								var rss = ds.TryGetValue(m, out var x) ? x : (ds[m] = new());
								rss[r] = l;
							}
						}
					}
				}
			}

			if(ds.Count == 0 && us.Count == 0)
			{
				Node.Peering.Statistics.Declaring.End();
				Thread.Sleep(1000);
				continue;
			}

			if(tasks.Count >= 32)
			{
				Task.WaitAny(tasks.Values.ToArray(), Node.Flow.Cancellation);
			}

			lock(Lock)
			{
				foreach(var r in us)
				{
					var t = Task.Run(() =>	{
												try
												{
													var cr = Node.Peering.Call(() => new ResourceRequest {Identifier = new(r.Address)}, Node.Flow);
													
													lock(Lock)
													{
														r.Id = cr.Resource.Id;
														r.Save();

														tasks.Remove(r);
													}
												}
												catch(NetException) ///when(!Debugger.IsAttached)
												{
												}
												catch(OperationCanceledException)
												{
													return;
												}
											});
					tasks[r] = t;
				}

				foreach(var i in ds)
				{
					foreach(var r in i.Value.Select(i => i.Value))
					{
						var d = r.DeclaredOn.Find(j => j.Member.Address == i.Key.Address);

						if(d == null)
							r.DeclaredOn.Add(new Declaration {Member = i.Key, Status = DeclarationStatus.InProgress});
						else
							d.Status = DeclarationStatus.InProgress;
					}

					var t = Task.Run(() =>	{
												DeclareReleaseResponse drr;

												try
												{
													drr = Node.Peering.Call(i.Key.SeedHubRdcIPs.Random(), () => new DeclareReleaseRequest {Resources = i.Value.Select(rs => new ResourceDeclaration{Resource = rs.Key.Id, 
																																																	Release = rs.Value.Address, 
																																																	Availability = rs.Value.Availability }).ToArray()}, Node.Flow);
												}
												catch(NodeException)/// when(!Debugger.IsAttached)
												{
													return;
												}
												catch(OperationCanceledException)
												{
													return;
												}

												lock(Lock)
												{
													foreach(var r in drr.Results)
													{	
														var x = Find(r.Address).DeclaredOn.Find(j => j.Member.Address == i.Key.Address);

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

													tasks.Remove(i.Key.Address);
												}
											});
					tasks[i.Key.Address] = t;
				}
			}
				
			Node.Peering.Statistics.Declaring.End();
		}
	}

	public FileDownload DownloadFile(LocalRelease release, bool single, string path, string localpath, IIntegrity integrity, SeedSeeker seeker, Flow workflow)
	{
		var f = release.Files.Find(i => i.Path == path);
		
		if(f != null)
		{
			if(f.Activity is FileDownload d0)
				return d0;
			else if(f.Activity != null)
				throw new ResourceException(ResourceError.Busy);
		}

		var d = new FileDownload(Node, release, single, path, localpath, integrity, seeker, workflow);
	
		return d;
	}

	public DirectoryDownload DownloadDirectory(LocalRelease release, string localpath, IIntegrity integrity, Flow workflow)
	{
		if(release.Activity is DirectoryDownload d)
			return d;
		else if(release.Activity != null)
			throw new ResourceException(ResourceError.Busy);
			
		d = new DirectoryDownload(Node, release, localpath, integrity, workflow);

		return d;
	}
}
