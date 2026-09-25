using System.IO.Compression;
using System.Text;
using RocksDbSharp;
using Uccs.Rdn;

namespace Uccs.Nexus;

public class PackageHub
{
	//public const string			FamilyName = nameof(Packages);

	public List<Package>	Packages = new();
	public RdnNode				Node;
	public object				Lock = new object();
	public string				DeploymentPath;
	//public ColumnFamilyHandle	Family => Node.Database.GetColumnFamily(FamilyName);

	public PackageHub(RdnNode node, string deploymentpath)
	{
		Node = node;
		DeploymentPath = deploymentpath;

		Directory.CreateDirectory(deploymentpath);
		
		//if(!Node.Database.TryGetColumnFamily(FamilyName, out var cf))	
		//	Node.Database.CreateColumnFamily(new (), FamilyName);
		//
		//using(var i = Node.Database.NewIterator(Family))
		//{
		//	for(i.SeekToFirst(); i.Valid(); i.Next())
		//	{
 		//		Packages.Add(new Reader(i.Key()) );
		//	}
		//}
	}

 	public static string AddressToDeployment(string packagespath, AutoId resource)
 	{
 		return Path.Join(packagespath, resource.ToString());
 	}

 	public string AddressToReleases(Urn release)
 	{
 		return Path.Join(Node.Settings.Seed.Releases, Net.Net.Escape(release.ToString()));
 	}

//	IEnumerable<LocalPackage> PreviousIncrementals(Ura package, Ura incrementalminimal)
//	{
//		return Find(package).Manifest.History	//.TakeWhile(i => !i.SequenceEqual(package.Hash))
//												.SkipWhile(i => i != incrementalminimal)
//												.Select(i => Find(i))
//												.Where(i => i is not null);
//	}

	public bool IsAvailable(AutoId package)
	{
		var p = Find(package);
		
		if(p == null)
			return false;

		if(p.Release == null)
			return false;

		lock(Node.ResourceHub.Lock)
		{
			if(	p.Release.Availability.HasFlag(Availability.Complete) || 
				p.Release.Availability.HasFlag(Availability.Incremental) && p.Manifest.Parents.Any(i => IsAvailable(i.Id)))
			{
				return p.Manifest.CriticalDependencies.All(i => IsAvailable(i.Id));
			}
			else
				return false;
		}
	}

//	public LocalPackage Get(AutoId resource)
//	{
//		var p = Find(resource);
//
//		if(p != null)
//			return p;
//
//		lock(Node.ResourceHub.Lock)
//		{
//			var r = Node.ResourceHub.Find(resource) ?? Node.ResourceHub.Add(resource);
//			p = new LocalPackage(this, r);
//		}
//
//		Packages.Add(p);
//
//		return p;
//	}

// 	public LocalPackage Find(Ura resource)
// 	{
// 		var p = Packages.Find(i => i.Resource.Address == resource);
// 
// 		if(p != null)
// 			return p;
// 
// 		LocalResource r;
// 
// 		lock(Node.ResourceHub.Lock)
// 		{
// 			r = Node.ResourceHub.Find(resource);
// 
// 			if(r != null)
// 			{
// 				p = new LocalPackage(this, r);
// 	
// 				Packages.Add(p);
// 	
// 				return p;
// 			}
// 		}
// 
// 		return null;
// 	}

 	public Package Find(AutoId id)
 	{
 		var p = Packages.Find(i => i.Id == id);
 
 		if(p != null)
 			return p;
 
 		ResourceData d;
 		
 		lock(Node.ResourceHub.Lock)
 		{
 			d = Node.ResourceHub.Get(id);
 		
 			if(d != null && d.Meaning == Meaning.Package_Software_VersionManifest)
 			{
				var m = d.Read<PackageManifest>();

 				if(Node.ResourceHub.Find(m.Urn) != null)
 				{
	 				p = new Package(this);
						
					p.Id = id;
	 		
	 				Packages.Add(p);
	 		
	 				return p;
 				}
 			}
 		}
 
 		return null;
 	}

// 		public LocalPackage Find(PackageAddress package)
// 		{
// 			var p = Packages.Find(i => i.Address == package);
// 
// 			if(p != null)
// 				return p;
// 
// 			lock(Sun.ResourceHub.Lock)
// 			{
// 				var rs = Sun.ResourceHub.Find(package);
// 				var rl = Sun.ResourceHub.Find(package.Release);
// 
// 				if(rs != null && rl != null)
// 				{
// 					p = new LocalPackage(this, package, rs, rl);
// 
// 					Packages.Add(p);
// 
// 					return p;
// 				}
// 			}
// 
// 			return null;
// 		}

	public bool ExistsRecursively(AutoId release)
	{
		var p = Find(release);

		if(p?.Manifest == null)
			return false;

		foreach(var i in p.Manifest.CompleteDependencies)
		{
			if(i.Need == DependencyNeed.Critical && !ExistsRecursively(i.Id))
				return false;
		}

		return true;
	}

	public void Build(Stream stream, Dictionary<string, string> files, IEnumerable<string> removals, Dictionary<string, byte[]> patches, Flow flow)
	{
		using(var arch = new ZipArchive(stream, ZipArchiveMode.Create, true))
		{
			foreach(var f in files)
			{
				arch.CreateEntryFromFile(f.Key, f.Value);
				flow.Log?.Report(this, "Packed", f.Value);
			}

			if(removals.Any())
			{
				var e = arch.CreateEntry(Package.Removals);
				var f = string.Join('\n', removals);
					
				using var s = e.Open();
				s.Write(Encoding.UTF8.GetBytes(f));
			}

			if(patches.Any())
			{
				var e = arch.CreateEntry(Package.Patches);
					
				using var s = e.Open();
				var w = new Writer(s);

				foreach(var i in patches)
				{
					w.WriteUtf8(i.Key);
					w.WriteBytes(i.Value);
				}
			}
		}
	}
	
	public void BuildIncremental(Stream stream, Release complete, IDictionary<string, string> all, Flow flow)
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
				foreach(var e in pzip.Entries)
				{
					olds.Add(e.FullName);

					var f = all.FirstOrDefault(i => i.Value == e.FullName);
				
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
						rems.Add(e.FullName);
						flow.Log?.Report(this, "Removed", e.FullName);
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

	public void DetermineDelta(PackageManifest manifest, out string file, out List<Dependency> dependencies)
	{
		var from = manifest.Parents?.LastOrDefault(i => IsAvailable(i.Id));
	
		if(from != null)
		{
			var deps = Find(from.Id).Manifest.CompleteDependencies.ToList();

			deps.AddRange(from.AddedDependencies);
			deps.RemoveAll(i => from.RemovedDependencies.Contains(i));
							
			deps.AddRange(manifest.CompleteDependencies.Where(i => !deps.Contains(i)));
			deps.RemoveAll(i => !manifest.CompleteDependencies.Contains(i));
				
			dependencies = deps;
			file = Package.DeltaFile; /// we have all incremental packages since last complete one
		}
		else
		{
			dependencies = manifest.CompleteDependencies.ToList();
			file = Package.CompleteFile;
		}
	}

	public Package BuildRelease(IEnumerable<string> sources, PackageManifest manifest, PackageInstruction instruction, AutoId previous, ReleaseAddressCreator addresscreator, Flow flow)
	{
		byte[] completed;
		byte[] delta = null;

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

		var ms = new MemoryStream();
		Build(ms, files, [], [], flow);
		completed = ms.ToArray();

		if(previous != null)
		{
			ms = new MemoryStream();
			BuildIncremental(ms, Find(previous).Release, files, flow);
			delta = ms.ToArray();
		}
		
	 	var p = new Package(this);

		p.Manifest = manifest;
	 	
	 	Packages.Add(p);
					
 		lock(Node.ResourceHub.Lock)
 		{
			/// pi.CompleteHash		= Node.ResourceHub.Net.Cryptography.HashFile(cstream);
			/// pi.IncrementalHash	= istream != null ? Node.ResourceHub.Net.Cryptography.HashFile(istream) : null;

			if(previous != null) /// a single parent supported only
			{
				var vm = Find(previous).Manifest;
			
				var d = new ParentPackage
						{
							Id					= previous,
							AddedDependencies	= manifest.CompleteDependencies.Where(i => !vm.CompleteDependencies.Contains(i)).ToArray(),
							RemovedDependencies	= vm.CompleteDependencies.Where(i => !manifest.CompleteDependencies.Contains(i)).ToArray() 
						};
				
				manifest.Parents = [d];
			}

			var x = new Dictionary<object, string>();
 			
			x[completed] = Package.CompleteFile;

			if(delta != null)
				x[delta] = Package.DeltaFile;

			if(instruction != null)
				x[(instruction as IBinarySerializable).ToRaw()] = PackageInstruction.Extension;

			var r = Node.ResourceHub.Add(x);

			r.Complete(Availability.Complete|(delta != null ? Availability.Incremental : 0));

			manifest.Urn = r.Address;

			flow.Log?.Report(this, $"Release built: {r.Address}");

			return p;
 		}
	}
 
//  		public Urr AddRelease(Ura resource, IEnumerable<string> sources, string dependenciespath, ReleaseAddressCreator addresscreator, Flow flow)
//  		{
//  			var r = Node.ResourceHub.Find(resource);
//  			var m = new PackageManifest();
//  		
//  			if(r != null)
//  			{
//  				var c = Node.ResourceHub.Find(r.LastAs<Urr>());
//  				m.Read(new BinaryReader(new MemoryStream(c.Find(LocalPackage.ManifestFile).Read())));
//  			}
//  		
//  			 return AddRelease(resource, sources, dependenciespath, m.History, m.History?.LastOrDefault(), addresscreator, flow);
//  		}

	public void StartDeploy(AutoId address, string packagespath, Flow flow)
	{
		lock(Lock)
		{
			var p = Find(address);

			if(p == null || !IsAvailable(address))
			{
				StartDownload(address, flow).Task.ContinueWith(t =>	{
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
			
			var pi = Path.Join(AddressToDeployment(packagespath, address), PackageInstruction.Extension);

			if(File.Exists(pi) && Bytes.Equal(p.Release.Hashify(PackageInstruction.Extension), Node.Net.Cryptography.HashFile(File.ReadAllBytes(pi))))
				return;

			var	d = new Deployment();

			void collect(Package parent, AutoId address){
															var m = new DeploymentMerge {Target = Find(address)};
															d.Merges.Add(m);

															var p = m.Target;

															while(flow.Active)
															{
																if(p.Release == null)
																{
																}
																else if(p.Release.Availability.HasFlag(Availability.Complete))
																{
																	if(p.Activity == null)
																		p.Activity = d;
																	else
																		throw new ResourceException(ResourceError.Busy);
				
																	m.Complete = p;

																	break;
																}
																else if(p.Release.Availability.HasFlag(Availability.Incremental))
																{	
																	if(p.Activity == null)
																		p.Activity = d;
																	else
																		throw new ResourceException(ResourceError.Busy);

																	var pp = p.Manifest.Parents.LastOrDefault(i => ExistsRecursively(i.Id));

																	if(pp == null)
																		throw new ResourceException(ResourceError.ParentPackagesNotFound);

																	m.Incrementals.Insert(0, new (p, pp));

																	p = Find(pp.Id);
																}

																Thread.Sleep(10);
															}

															//all.AddRange(s.Select(i => i.Key).AsEnumerable().Reverse());
			
															var deps = new HashSet<AutoId>();

															foreach(var j in m.Complete.Manifest.CompleteDependencies.Where(i => i.Need == DependencyNeed.Critical))
																deps.Add(j.Id);

															foreach(var i in m.Incrementals.AsEnumerable().Reverse())
															{
																foreach(var j in i.Value.AddedDependencies.Where(i => i.Need == DependencyNeed.Critical))
																	deps.Add(j.Id);
	
																foreach(var j in i.Value.RemovedDependencies)
																	deps.Remove(j.Id);
															}

															foreach(var i in deps)
																collect(p, i);
														}

			collect(null, address);

			Task.Run(() =>	{ 
								foreach(var s in d.Merges.AsEnumerable().Reverse())
								{
									using(var fs = new FileStream(s.Complete.Release.Find(Package.CompleteFile).DataPath, FileMode.Open))
									{
										using(var a = new ZipArchive(fs, ZipArchiveMode.Read))
										{
											foreach(var e in a.Entries)
											{
												var f = Path.Join(AddressToDeployment(packagespath, s.Target.Id), e.FullName.Replace('/', Path.DirectorySeparatorChar));
								
												Directory.CreateDirectory(Path.GetDirectoryName(f));
												e.ExtractToFile(f, true);
											}
										}
									}

									foreach(var i in s.Incrementals)
									{
										using(var fs = new FileStream(i.Key.Release.Find(Package.DeltaFile).DataPath, FileMode.Open))
										{
											using(var z = new ZipArchive(fs, ZipArchiveMode.Read))
											{
												foreach(var e in z.Entries)
												{
													if(e.Name != Package.Removals)
													{
														var f = Path.Join(AddressToDeployment(packagespath, s.Target.Id), e.FullName.Replace('/', Path.DirectorySeparatorChar));
								
														Directory.CreateDirectory(Path.GetDirectoryName(f));
														e.ExtractToFile(f, true);
													} 
													else
													{
														using(var es = e.Open())
														{
															var sr = new StreamReader(es);

															while(!sr.EndOfStream)
															{
																File.Delete(Path.Join(AddressToDeployment(packagespath, s.Target.Id), sr.ReadLine().Replace('/', Path.DirectorySeparatorChar)));
															}
														}
													}
												}
											}
										}

										i.Key.Activity = null;
									}

									//File.WriteAllText(Path.Join(todeployment(s.Target.Resource.Address), ".hash"), s.Target.Manifest.CompleteHash.ToHex());

									foreach(var i in s.Complete.Manifest.CompleteDependencies.Where(i => i.Need == DependencyNeed.Critical && i.Flags.HasFlag(DependencyFlag.Merge)))
									{
										var d = Find(i.Id);
									
										while(d == null || d.Activity != null)
											if(flow.Active)
												Thread.Sleep(100);
											else
												return;

										foreach(var fs in Directory.EnumerateFiles(AddressToDeployment(packagespath, i.Id), "*", SearchOption.AllDirectories).Where(i => Path.GetExtension(i) != PackageInstruction.Extension))
										{
											var fd = Path.Join(AddressToDeployment(packagespath, s.Target.Id), fs.Substring(AddressToDeployment(packagespath, i.Id).Length + 1));
											Directory.CreateDirectory(Path.GetDirectoryName(fd));

											File.Copy(fs, fd, true);
										}
										//var f = Path.Join(todeployment(s.Target.Resource.Address), e.FullName.Replace('/', Path.DirectorySeparatorChar));
									
									}

									File.Copy(s.Complete.Release.Find(PackageInstruction.Extension).DataPath, Path.Join(AddressToDeployment(packagespath, s.Target.Id), PackageInstruction.Extension), true);

									s.Complete.Activity = null;
								}
							});
		}
	}

	public PackageDownload StartDownload(AutoId id, Flow flow)
	{
		var p = Find(id);
		
		if(p == null)
		{
			p = new Package(this){Id = id};
			Packages.Add(p);
		}

		if(p.Activity is PackageDownload d)
			return d;
		else if(p.Activity != null)
			throw new ResourceException(ResourceError.Busy);
			
		d = new PackageDownload(this, p, flow);

		return d;
	}

	public Package Deploy(AutoId address, Flow flow)
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
