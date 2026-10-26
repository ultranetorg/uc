using System.Diagnostics;
using System.Text;
using RocksDbSharp;

namespace Uccs.Rdn;

public class ResourceHub
{
	public const long				PieceMaxLength = 512 * 1024;
	public const int				MembersPerDeclaration = 3;
	public const string				ReleaseFamilyName = nameof(Releases);
	//public const string				ResourceFamilyName = nameof(Resources);

	public List<LocalRelease>		Releases = new();
	public List<CachedResource>		Resources = new();
	public RdnNode					Node;
	public object					Lock = new object();
	public McvNet					Net;
	public ColumnFamilyHandle		ReleaseFamily => Node.Database.GetColumnFamily(ReleaseFamilyName);
	//public ColumnFamilyHandle		ResourceFamily => Node.Database.GetColumnFamily(ResourceFamilyName);
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
		//if(!Node.Database.TryGetColumnFamily(ResourceFamilyName, out cf))		Node.Database.CreateColumnFamily(new (), ResourceFamilyName);

		using(var i = Node.Database.NewIterator(ReleaseFamily))
		{
			for(i.SeekToFirst(); i.Valid(); i.Next())
			{
 				Releases.Add(new LocalRelease(this, Urn.FromRaw(i.Key())));
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
		return Path.Join(Settings.Releases, urr.ToString().EscapeFilePath());
	}

	public LocalRelease Add(Urn address)
	{
		if(Releases.Any(i => i.Address == address))
			throw new ResourceException(ResourceError.AlreadyExists);

		var r = new LocalRelease(this, address);

		Releases.Add(r);

		return r;
	}

	public LocalRelease Add(Dictionary<object, string> files)
	{
		var r = new LocalRelease(this, files);

		if(Releases.Any(i => i.Address == r.Address))
			throw new ResourceException(ResourceError.AlreadyExists);

		Releases.Add(r);

		return r;
	}

	public LocalRelease Find(Urn address)
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

	public CachedResource Get(AutoId id)
	{
		var c = Resources.Find(i => i.Id == id);

		if(c != null)
		{	
			if(DateTime.UtcNow - c.Updated < TimeSpan.FromSeconds(10))
				return c;
			else
			{	
				var r = Node.Peering.Call(new ResourceByIdPpc(id), Node.Flow);

				c.Address	= r.Address;
				c.Data		= r.Resource.Data;
				c.Updated	= DateTime.UtcNow;
			}
		}
		else
		{
			var r = Node.Peering.Call(new ResourceByIdPpc(id), Node.Flow); 

			c = new CachedResource
				{
					Id		= r.Resource.Id,
					Address = r.Address,
					Data	= r.Resource.Data,
					Updated = DateTime.UtcNow
				};

			Resources.Add(c);
		}

		return c;
	}

	public CachedResource Get(Ura address)
	{
		var c = Resources.Find(i => i.Address == address);

		if(c != null)
		{	
			if(DateTime.UtcNow - c.Updated < TimeSpan.FromSeconds(10))
				return c;
			else
			{	
				var r = Node.Peering.Call(new ResourceByAddressPpc(address), Node.Flow);

				c.Id		= r.Resource.Id;
				c.Data		= r.Resource.Data;
				c.Updated	= DateTime.UtcNow;
			}
		}
		else
		{
			var r = Node.Peering.Call(new ResourceByAddressPpc(address), Node.Flow); 

			c = new CachedResource
				{
					Id		= r.Resource.Id,
					Address = address,
					Data	= r.Resource.Data,
					Updated = DateTime.UtcNow
				};

			Resources.Add(c);
		}

		return c;
	}

	public LocalRelease Add(IEnumerable<string> sources, ReleaseAddressCreator address, Flow workflow)
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

	public LocalRelease Add(string localpath, ReleaseAddressCreator address, Flow workflow)
	{
		using var b = File.OpenRead(localpath);

		var h = Net.Cryptography.HashFile(b);
		var a = address.Create(null/*Node.Vault*/, h);
 			
		var r = Add(a);

 		r.AddCompleted("", localpath, null);
		r.Complete(Availability.Full);

		return r;
	}

	public ReleaseFile GetFile(LocalRelease release, bool single, string file, string localpath, IIntegrity integrity, SeedSeeker harvester, Flow workflow)
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

			var ds = new Dictionary<RdnGenerator, List<LocalRelease>>();

			lock(Lock)
			{
				foreach(var r in Releases.Where(i => i.Availability != Availability.None))
				{
					foreach(var m in mr.Members	.OrderByHash(i => i.Generator.Raw, r.Address.MemberOrderKey)
												.Take(MembersPerDeclaration)
												.Where(m =>	{
																var d = r.DeclaredOn.Find(dm => dm.Member.Generator == m.Generator);
																return d == null || d.Status == DeclarationStatus.Failed && DateTime.UtcNow - d.Failed > TimeSpan.FromSeconds(3);
															})
												.Cast<RdnGenerator>())
					{
						var rss = ds.TryGetValue(m, out var x) ? x : (ds[m] = []);
						rss.Add(r);
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
					foreach(var r in i.Value)
					{
						var d = r.DeclaredOn.Find(j => j.Member.Generator == i.Key.Generator);

						if(d == null)
							r.DeclaredOn.Add(new Declaration {Member = i.Key, Status = DeclarationStatus.InProgress});
						else
							d.Status = DeclarationStatus.InProgress;
					}

					var t = Task.Run(() =>	{
												ReleaseDeclarationPpr drr;
												ReleaseDeclaration[] rds;

												lock(Lock)
													rds = i.Value.Select(rs =>	new ReleaseDeclaration
																				{
																					Urn				= rs.Address, 
																					Availability	= rs.Availability
																				})
																				.ToArray();

												try
												{
													drr = Node.Peering.Call(i.Key.SeedhubPpiEndpoints.Random(), new ReleaseDeclarationPpc {Resources = rds}, Node.Flow);
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
