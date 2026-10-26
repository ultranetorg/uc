using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using RocksDbSharp;
using Uccs.Rdn;

namespace Uccs.Nexus;

public class PackageHub
{
	public const string			FamilyName = nameof(Packages);

	public List<Package>		Packages = new();
	public RdnNode				Node;
	public object				Lock = new object();
	public string				DeploymentPath;
	public ColumnFamilyHandle	Family => Node.Database.GetColumnFamily(FamilyName);

	public PackageHub(RdnNode node, string deploymentpath)
	{
		Node = node;
		DeploymentPath = deploymentpath;

		Directory.CreateDirectory(deploymentpath);
		
		if(!Node.Database.TryGetColumnFamily(FamilyName, out var cf))	
			Node.Database.CreateColumnFamily(new (), FamilyName);
		
		using(var i = Node.Database.NewIterator(Family))
		{
			for(i.SeekToFirst(); i.Valid(); i.Next())
			{
				var v = new Reader(i.Value());

 				Packages.Add(new Package(this)
							 {
								Address	 = new Reader(i.Key()).Read<Ura>(),
								Urn		 = v.Read<Urn>(),
							 });
			}
		}
	}

	public void Save(Package package)
	{
		using var b = new WriteBatch();

		var s = new MemoryStream();
		var w = new Writer(s, Rdn.Rdn.Any.Constructor);

		w.Write(package.Urn);

		b.Put((package.Address as IBinarySerializable).ToRaw(), s.ToArray());

		Node.Database.Write(b);
	}

 	public static string AddressToDeployment(string packagespath, Ura resource)
 	{
 		return Path.Join(packagespath, resource.ToString().EscapeFilePath());
 	}

 	public static string AddressToDeployment(string packagespath, Ura resource, string other)
 	{
 		return Path.Join(packagespath, resource.ToString().EscapeFilePath(), other);
 	}

 	public string AddressToReleases(Urn release)
 	{
 		return Path.Join(Node.Settings.Seed.Releases, release.ToString().EscapeFilePath());
 	}

	public bool IsReady(Ura package)
	{
		var p = Find(package);
		
		if(p == null)
			return false;

		if(p.Release == null)
			return false;

		if(p.Manifest == null)
			return false;

		lock(Node.ResourceHub.Lock)
		{
			if(	p.Release.Availability.HasFlag(Availability.Complete) || 
				p.Release.Availability.HasFlag(Availability.Delta) && p.Manifest.Parents.Any(i => IsReady(i.Address)))
			{
				return p.Manifest.Criticals.All(i => IsReady(i.Address));
			}
			else
				return false;
		}
	}

 	public Package Find(Ura address)
 	{
 		var p = Packages.Find(i => i.Address == address);
 
 		return p;
 	}

//	public bool ExistsRecursively(Ura release)
//	{
//		var p = Find(release);
//
//		if(p?.Manifest == null)
//			return false;
//
//		foreach(var i in p.Manifest.CompleteDependencies)
//		{
//			if(i.Need == DependencyNeed.Critical && !ExistsRecursively(i.Address))
//				return false;
//		}
//
//		return true;
//	}

	public void Build(Stream stream, Dictionary<string, string> files, IEnumerable<string> removals, Dictionary<string, byte[]> patches, Flow flow)
	{
		using var arch = new ZipArchive(stream, ZipArchiveMode.Create, true);
		
		foreach(var f in files)
		{
			arch.CreateEntryFromFile(f.Key, $"{Package.New}{f.Value}");
			flow.Log?.Report(this, "Packed", f.Value);
		}

		if(removals.Any())
		{
			var e = arch.CreateEntry(Package.Deletion);
			var f = string.Join('\n', removals);
					
			using var s = e.Open();
			s.Write(Encoding.UTF8.GetBytes(f));
		}

		if(patches.Any())
		{
			var e = arch.CreateEntry(Package.Patch);
					
			using var s = e.Open();
			var w = new Writer(s);

			foreach(var i in patches)
			{
				w.WriteUtf8(i.Key);
				w.WriteBytes(i.Value);
			}
		}
	}
	
	void BuildIncremental(Stream stream, LocalRelease complete, IDictionary<string, string> all, Flow flow)
	{
		var rems = new List<string>();
		var upds = new Dictionary<string, string>();
		var pchs = new Dictionary<string, byte[]>();
		var olds = new HashSet<string>();

		//var prev = package.ReplaceHash(previous);

		string ppath;
			
		lock(Node.ResourceHub)
			ppath = complete.Find(Package.CompleteFile).DataPath;

		using(var ps = new FileStream(ppath, FileMode.Open))
		{
			using(var pzip = new ZipArchive(ps, ZipArchiveMode.Read))
			{
				foreach(var e in pzip.Entries.Where(i => i.FullName.StartsWith(Package.New)))
				{
					var path = e.FullName.Substring(Package.New.Length);

					olds.Add(path);

					var f = all.FirstOrDefault(i => i.Value == path);
				
					if(f.Key != null) /// new package contains a file from the old one
					{
						bool updated = e.Length != new FileInfo(f.Key).Length;

						if(!updated)
						{
							using var a = e.Open();
							using var b = File.OpenRead(f.Key);

							var abuffer = new byte[e.Length];
							var bbuffer = new byte[e.Length];

							var n = a.Read(abuffer, 0, (int)e.Length);

							while(n < e.Length)
								n += a.Read(abuffer, n, (int)e.Length - n);

							var m = b.Read(bbuffer, 0, (int)e.Length);

							while(m < e.Length)
								m += b.Read(bbuffer, m, (int)e.Length - m);
															
							updated = !Bytes.Equal(abuffer, bbuffer);
						}

						if(updated)
						{
							upds.Add(f.Key, f.Value);
							flow.Log?.Report(this, "Updated", f.Value);
						}
					}
					else /// a file is removed in the new package
					{
						rems.Add(path);
						flow.Log?.Report(this, "Removed", path);
					}
				}
			}
		}

		foreach(var f in all)
		{
			if(!olds.Contains(f.Value)) /// a completely new file
			{
				upds.Add(f.Key, f.Value);
				flow.Log?.Report(this, "New", f.Value);
			}
		}
		
		Build(stream, upds, rems, pchs, flow);
	}

	public void DetermineDelta(PackageManifest manifest, out string file)
	{
		var from = manifest.Parents?.LastOrDefault(i => IsReady(i.Address));
	
		if(from != null)
		{
			//var deps = Find(from.Address).Manifest.CompleteDependencies.ToList();
			//
			//deps.AddRange(from.AddedDependencies);
			//deps.RemoveAll(i => from.RemovedDependencies.Contains(i));
			//				
			//deps.AddRange(manifest.CompleteDependencies.Where(i => !deps.Contains(i)));
			//deps.RemoveAll(i => !manifest.CompleteDependencies.Contains(i));
				
			file = from.Address.ToString().EscapeFilePath() + "." + Package.DeltaExtention; /// we have all delta packages since last complete one
		}
		else
		{
			file = Package.CompleteFile;
		}
	}

	public Package Create(Ura address, IEnumerable<string> sources, PackageManifest manifest, PackageInstruction instruction, ReleaseAddressCreator addresscreator, Flow flow)
	{
		byte[] completed;

		var files = new Dictionary<string, string>();

		foreach(var i in sources)
		{
			var sd = i.Split('=');
			var s = sd[0];
			var d = sd.Length == 2 ? sd[1] : null;

			if(d == null)
			{
				if(Directory.Exists(s))
				{
					foreach(var e in Directory.EnumerateFiles(s, "*", new EnumerationOptions{RecurseSubdirectories = true}))
						files[e] = e.Substring(s.Length + 1).Replace(Path.DirectorySeparatorChar, '/');
				}
				else
					files[s] = Path.GetFileName(s);
			}
			else
			{
				if(Directory.Exists(s))
				{
					foreach(var e in Directory.EnumerateFiles(s, "*", new EnumerationOptions{RecurseSubdirectories = true}))
						files[e] = Path.Join(d, e.Substring(s.Length + 1).Replace(Path.DirectorySeparatorChar, '/'));
				}
				else
					files[s] = d;
			}
		}

		var cs = new MemoryStream();
		Build(cs, files, [], [], flow);
		completed = cs.ToArray();

		if(manifest.Parents.Length > 1)
			throw new PackageException(PackageError.NotSupported, "A single parent supported only");

		var x = new Dictionary<object, string>();

		foreach(var i in manifest.Parents) /// a single parent supported only
		{
			var parent = Find(i.Address);

			var ds = new MemoryStream();

			BuildIncremental(ds, parent.Release, files, flow);

			//i.AddedDependencies		= manifest.CompleteDependencies.Where(i => !parent.Manifest.CompleteDependencies.Contains(i)).ToArray();
			//i.RemovedDependencies	= parent.Manifest.CompleteDependencies.Where(i => !manifest.CompleteDependencies.Contains(i)).ToArray();

			x[ds.ToArray()] = parent.Address.ToString().EscapeFilePath() + "." + Package.DeltaExtention;
		}
		
	 	var p = new Package(this);

		p.Address = address;
		//p.Manifest = manifest;
	 	
	 	Packages.Add(p);

		x[(manifest as IBinarySerializable).ToRaw(Rdn.Rdn.Any.Constructor)] = PackageManifest.Extension;
		x[completed] = Package.CompleteFile;

		if(instruction != null)
			x[(instruction as IBinarySerializable).ToRaw()] = PackageInstruction.Extension;
					
 		lock(Node.ResourceHub.Lock)
 		{
			var r = Node.ResourceHub.Add(x);

			r.Complete(Availability.Complete|(manifest.Parents.Any() ? Availability.Delta : 0));

			p.Urn = r.Address;

			flow.Log?.Report(this, $"Release built: {r.Address}");

 		}

		Save(p);

		return p;
	}

	public void StartDeploy(Ura address, string packagespath, Flow flow)
	{
		packagespath ??= DeploymentPath;

		lock(Lock)
		{
			var p = Find(address);

			if(p == null || !IsReady(address))
			{
				StartDownload(address, true, flow).Task.ContinueWith(t =>	{
																				lock(Lock)
																					deploy();
																			});
			}
			else
				deploy();
		}

		void deploy()
		{
			var p = Find(address);

			if(p.IsDeployed(packagespath))
				return;

			var	d = new Deployment();

			void collect(Ura address)
			{
				var m = new DeploymentMerge {Target = Find(address)};
				d.Merges.Add(m);

				var t = m.Target;

				while(flow.Active)
				{
					if(t.Release == null)
					{
					}
					else if(t.Release.Availability.HasFlag(Availability.Complete))
					{
						if(t.Activity == null)
							t.Activity = d;
						else
							continue;
				
						m.Complete = t;

						break;
					}
					else if(t.Release.Availability.HasFlag(Availability.Delta))
					{	
						if(t.Activity == null)
							t.Activity = d;
						else
							continue;

						var p = t.Manifest.Parents.LastOrDefault(i => IsReady(i.Address));

						if(p == null)
							continue;;

						m.Deltas.Insert(0, new (t, p));

						t = Find(p.Address);
					}

					Monitor.Exit(Lock);
					Thread.Sleep(10);
					Monitor.Enter(Lock);
				}
			}

			collect(p.Address);

			foreach(var i in p.Manifest.Criticals)
				collect(i.Address);

			Task.Run(() =>	{ 
								foreach(var s in d.Merges.AsEnumerable().Reverse())
								{
									using var cfs = new FileStream(s.Complete.Release.Find(Package.CompleteFile).DataPath, FileMode.Open);
									using var ca = new ZipArchive(cfs, ZipArchiveMode.Read);
									
									var deplpath = AddressToDeployment(packagespath, s.Target.Address); 

									foreach(var e in ca.Entries) /// extract from complete
									{
										var f = Path.Join(deplpath, e.FullName.Substring(Package.New.Length).Replace('/', Path.DirectorySeparatorChar));
								
										Directory.CreateDirectory(Path.GetDirectoryName(f));
										e.ExtractToFile(f, true);
									}

									var prev = s.Complete;

									foreach(var i in s.Deltas)
									{
										using var ifs = new FileStream(i.Key.Release.Find(prev.Address.ToString().EscapeFilePath() + "." + Package.DeltaExtention).DataPath, FileMode.Open);
										using var z = new ZipArchive(ifs, ZipArchiveMode.Read);
										
										foreach(var e in z.Entries.Where(i => i.FullName.StartsWith(Package.New)))
										{
											var f = Path.Join(deplpath, e.FullName.Substring(Package.New.Length).Replace('/', Path.DirectorySeparatorChar));
								
											Directory.CreateDirectory(Path.GetDirectoryName(f));
											e.ExtractToFile(f, true);
										}

										var d = z.GetEntry(Package.Deletion);
										
										if(d != null)
										{
											using var es = d.Open();
											var sr = new StreamReader(es);

											while(!sr.EndOfStream)
											{
												File.Delete(Path.Join(deplpath, sr.ReadLine().Replace('/', Path.DirectorySeparatorChar)));
											}
										}
									
										prev = i.Key;

										i.Key.Activity = null;
									}

									//File.WriteAllText(Path.Join(todeployment(s.Target.Resource.Address), ".hash"), s.Target.Manifest.CompleteHash.ToHex());

									foreach(var i in s.Complete.Manifest.Criticals.Where(i => i.Flags.HasFlag(DependencyFlag.Merge)))
									{
										var d = Find(i.Address);
									
										while(d == null || d.Activity != null)
											if(flow.Active)
												Thread.Sleep(100);
											else
												return;

										foreach(var j in Directory.EnumerateFiles(AddressToDeployment(packagespath, i.Address), "*", SearchOption.AllDirectories))
										{
											var fd = AddressToDeployment(packagespath, s.Target.Address, j.Substring(AddressToDeployment(packagespath, i.Address).Length + 1));
											Directory.CreateDirectory(Path.GetDirectoryName(fd));

											File.Copy(j, fd, true);
										}
										//var f = Path.Join(todeployment(s.Target.Resource.Address), e.FullName.Replace('/', Path.DirectorySeparatorChar));
									}

									s.Target.SetDeployed(packagespath);

									s.Complete.Activity = null;
								}
							});
		}
	}

	public PackageDownload StartDownload(Ura address, bool recursive, Flow flow)
	{
		var p = Find(address);
		
		if(p == null)
		{
			var r = Node.ResourceHub.Get(address);

			p = new Package(this)
				{
					Address	= r.Address,
					Urn		= r.Data.Get<Urn>(),
				};

			Packages.Add(p);
			Node.ResourceHub.Add(p.Urn);
		}

		if(IsReady(address))
			return new PackageDownload(this, p);

		if(p.Activity is PackageDownload d)
			return d;
		else if(p.Activity != null)
			throw new ResourceException(ResourceError.Busy);
			
		d = new PackageDownload(this, p, recursive, flow);

		return d;
	}

	public PackageDownload StartDownload(Dependency dependency, bool recursive, Flow flow)
	{
		var p = Find(dependency.Address);
		
		if(p == null)
		{
			p = new Package(this)
				{
					Address	= dependency.Address,
					Urn		= dependency.Name,
				};

			Packages.Add(p);
			Node.ResourceHub.Add(p.Urn);
		}

		if(IsReady(dependency.Address))
			return new PackageDownload(this, p);

		if(p.Activity is PackageDownload d)
			return d;
		else if(p.Activity != null)
			throw new ResourceException(ResourceError.Busy);
			
		d = new PackageDownload(this, p, recursive, flow);

		return d;
	}

	public Package Deploy(Ura address, Flow flow)
	{
		StartDeploy(address, DeploymentPath, flow);

		Package p;

		lock(Lock)
			p = Find(address);

		do
		{
			if(p.Activity is null)
				return p;

			Thread.Sleep(100);
		}
		while(flow.Active);

		throw new OperationCanceledException();
	}
}
