using System.Diagnostics;
using System.Text;
using RocksDbSharp;

namespace Uccs.Rdn;

public class ResourceHub
{
	public const long										PieceMaxLength = 512 * 1024;
	public const int										MembersPerDeclaration = 3;
	public const string										ReleaseFamilyName = nameof(Releases);
	public const string										ResourceFamilyName = nameof(Resources);

	public List<Release>								Releases = new();
	public List<CachedResource>								Resources = new();
	public RdnNode											Node;
	public object											Lock = new object();
	public McvNet											Net;
	public ColumnFamilyHandle								ReleaseFamily => Node.Database.GetColumnFamily(ReleaseFamilyName);
	public ColumnFamilyHandle								ResourceFamily => Node.Database.GetColumnFamily(ResourceFamilyName);
	public SeedSettings										Settings;
	Thread													DeclaringThread;

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
 				Releases.Add(new Release(this, Urn.FromRaw(i.Key())));
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

	public string ToReleases(Urn urr)
	{
		return Path.Join(Settings.Releases, Uccs.Net.Net.Escape(urr.ToString()));
	}

	public Release Add(Urn address, AutoId id)
	{
		if(Releases.Any(i => i.Address == address))
			throw new ResourceException(ResourceError.AlreadyExists);

		var r = new Release(this, address);
		r.Resource		= id;
		r.__StackTrace	= new System.Diagnostics.StackTrace(true);

		Releases.Add(r);

		return r;
	}

	public void Add(Release release)
	{
		if(Releases.Any(i => i.Address == release.Address))
			throw new ResourceException(ResourceError.AlreadyExists);

		Releases.Add(release);
	}

	public Release Add(Dictionary<object, string> files)
	{
		var r = new Release(this, files);

		if(Releases.Any(i => i.Address == r.Address))
			throw new ResourceException(ResourceError.AlreadyExists);

		Releases.Add(r);

		return r;
	}

//	public LocalResource Update(AutoId id, ResourceData data)
//	{
//		if(Ids.TryGetValue(id, out var d))
//		{
//			d.Data = data;
//			d.Updated = DateTime.UtcNow;
//		} 
//		else
//			Ids[id] = d = new LocalResource(this, data){Updated = DateTime.UtcNow};
//
//		return d;
//	}
//
//	public LocalResource Update(Ura address, ResourceData data)
//	{
//		if(Addresses.TryGetValue(address, out var d))
//		{
//			d.Data = data;
//			d.Updated = DateTime.UtcNow;
//		} 
//		else
//			Addresses[address] = d = new LocalResource(this, data){Updated = DateTime.UtcNow};
//
//		return d;
//	}

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

	public Release Find(Urn address)
	{
		var r = Releases.Find(i => i.Address == address);

		if(r != null)
			return r;

		var d = Node.Database.Get(address.Raw, ReleaseFamily);

		if(d != null)
		{
			r = new Release(this, address);
			Releases.Add(r);
			return r;
		}

		return null;
	}
//
//	public ResourceData Find(AutoId id)
//	{
//		var c = Resources.Find(i => i.Id == id);
//
//		if(c != null)
//		{	
//			return c.Data;
//		}
//		else
//		{
//			var d = Node.Database.Get(id.Raw, ResourceFamily);
//									
//			if(d != null)
//			{	
//				c = new CachedResource(id, new Reader(d).Read<ResourceData>());
//				Resources.Add(c);
//			}
//		}
//
//		return c.Data;
//	}

//	public ResourceData Get(Ura address)
//	{
//		var r = Resources.Find(i => i.Address == address);
//
//		if(r != null)
//		{	
//			if(DateTime.UtcNow - r.Updated < TimeSpan.FromSeconds(30))
//				return r.Data;
//			else
//				r.Data = Node.Peering.Call(new ResourceByAddressPpc(address), Node.Flow).Resource.Data;
//		}
//		else
//		{
//			var d = Node.Database.Get(id.Raw, ResourceFamily);
//									
//			if(d != null)
//			{
//				r = new CachedResource(id, new Reader(d).Read<ResourceData>());
//				Resources.Add(r);
//			}
//			else
//			{	
//				var a = Node.Peering.Call(new ResourceByIdPpc(id), Node.Flow).Resource; 
//				
//				r = new CachedResource(a);
//				Resources.Add(r);
//
//				if(r.Data != null && a.Flags.HasFlag(ResourceFlags.Dependable))
//				{
//					Node.Database.Put(id.Raw, (r.Data as IBinarySerializable).ToRaw(), ResourceFamily);
//				}
//			}
//		}
//
//		r.Updated = DateTime.UtcNow;
//
//		return r.Data;
//	}

	public ResourceData Get(AutoId id)
	{
		var r = Resources.Find(i => i.Id == id);

		if(r != null)
		{	
			if(DateTime.UtcNow - r.Updated < TimeSpan.FromSeconds(10))
				return r.Data;
			else
			{	
				r.Data = Node.Peering.Call(new ResourceByIdPpc(id), Node.Flow).Resource.Data;
				r.Updated = DateTime.UtcNow;
			}
		}
		else
		{
			var d = Node.Database.Get(id.Raw, ResourceFamily);
									
			if(d != null)
			{
				r = new CachedResource(id, new Reader(d).Read<ResourceData>());
				Resources.Add(r);
			}
			else
			{	
				var a = Node.Peering.Call(new ResourceByIdPpc(id), Node.Flow).Resource; 
				
				r = new CachedResource(a);
				r.Updated = DateTime.UtcNow;

				Resources.Add(r);

				if(r.Data != null && a.Flags.HasFlag(ResourceFlags.Dependable))
				{
					Node.Database.Put(id.Raw, (r.Data as IBinarySerializable).ToRaw(), ResourceFamily);
				}
			}
		}

		return r.Data;
	}

	public Release Add(IEnumerable<string> sources, ReleaseAddressCreator address, Flow workflow)
	{
		var files = new Dictionary<object, string>();

		void adddir(string basepath, string dir, string dest)
		{
			//var d = parent.Add(Path.GetFileName(path));

			foreach(var i in Directory.EnumerateFiles(dir))
			{
				files[i] = Path.Join(dest, i.Substring(basepath.Length + 1).Replace(Path.DirectorySeparatorChar, '/'));
			}

			foreach(var i in Directory.EnumerateDirectories(dir).Where(i => Directory.EnumerateFileSystemEntries(i).Any()))
			{
				adddir(basepath, i, dest);
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
					adddir(s, s, null);
				}
				else
				{
					files[s] = Path.GetFileName(s);
				}
			}
			else
			{
				if(Directory.Exists(s))
				{
					adddir(s, s, d);
				}
				else
				{
					files[s] = d;
				}
			}
		}

		var r = Add(files);
		r.Complete(Availability.Full);

		return r;
	}

	public Release Add(string localpath, ReleaseAddressCreator address, Flow workflow)
	{
		using var b = File.OpenRead(localpath);

		var h = Net.Cryptography.HashFile(b);
		var a = address.Create(null/*Node.Vault*/, h);
 			
		var r = Add(a, null);

 		r.AddCompleted("", localpath, null);
		r.Complete(Availability.Full);

		return r;
	}

	public ReleaseFile GetFile(Release release, bool single, string file, string localpath, IIntegrity integrity, SeedSeeker harvester, Flow workflow)
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
			RdnMembersPpr mr = null;

			try
			{
				mr = Node.Peering.Call(new RdnMembersPpc(), Node.Flow);
	
				if(mr == null) 
				{	
					Debugger.Break();
					mr = Node.Peering.Call(new RdnMembersPpc(), Node.Flow);
					continue;
				}
	
				if(!mr.Members.Any())
					continue;
			}
			catch(Exception)
			{
				continue;
			}

			var ds = new Dictionary<RdnGenerator, Dictionary<AutoId, Release>>();

			lock(Lock)
			{
				foreach(var r in Releases.Where(i => i.Resource != null))
				{
					if(r.Availability != Availability.None)
					{
						foreach(var m in mr.Members	.OrderByHash(i => i.Generator.Raw, r.Address.MemberOrderKey)
													.Take(MembersPerDeclaration)
													.Where(m =>	{
																	var d = r.DeclaredOn.Find(dm => dm.Member.Generator == m.Generator);
																	return d == null || d.Status == DeclarationStatus.Failed && DateTime.UtcNow - d.Failed > TimeSpan.FromSeconds(3);
																})
													.Cast<RdnGenerator>())
						{
							var rss = ds.TryGetValue(m, out var x) ? x : (ds[m] = new());
							rss[r.Resource] = r;
						}
					}
				}
			}

			if(ds.Count == 0)
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
				foreach(var i in ds)
				{
					foreach(var r in i.Value.Select(i => i.Value))
					{
						var d = r.DeclaredOn.Find(j => j.Member.Generator == i.Key.Generator);

						if(d == null)
							r.DeclaredOn.Add(new Declaration {Member = i.Key, Status = DeclarationStatus.InProgress});
						else
							d.Status = DeclarationStatus.InProgress;
					}

					var t = Task.Run(() =>	{
												DeclareReleasePpr drr;
												ResourceDeclaration[] rds;

												lock(Lock)
													rds = i.Value.Select(rs =>	new ResourceDeclaration
																				{
																					Resource		= rs.Key, 
																					Release			= rs.Value.Address, 
																					Availability	= rs.Value.Availability
																				})
																				.ToArray();

												try
												{
													drr = Node.Peering.Call(i.Key.SeedhubPpiEndpoints.Random(), new DeclareReleasePpc {Resources = rds}, Node.Flow);
												}
												catch(CodeException)/// when(!Debugger.IsAttached)
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
														var x = Find(r.Address).DeclaredOn.Find(j => j.Member.Generator == i.Key.Generator);

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

													tasks.Remove(i.Key.Generator);
												}
											});
					tasks[i.Key.Generator] = t;
				}
			}
				
			Node.Peering.Statistics.Declaring.End();
		}
	}

	public FileDownload DownloadFile(Release release, bool single, string path, string localpath, IIntegrity integrity, SeedSeeker seeker, Flow workflow)
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

	public DirectoryDownload DownloadDirectory(Release release, string localpath, IIntegrity integrity, Flow workflow)
	{
		if(release.Activity is DirectoryDownload d)
			return d;
		else if(release.Activity != null)
			throw new ResourceException(ResourceError.Busy);
			
		d = new DirectoryDownload(Node, release, localpath, integrity, workflow);

		return d;
	}
}
