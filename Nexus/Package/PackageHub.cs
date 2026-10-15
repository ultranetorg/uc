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
								Id		 = v.ReadNullable<AutoId>(),
								Manifest = v.Read<PackageManifest>(),
							 });
			}
		}
	}

	public void Save(Package package)
	{
		using var b = new WriteBatch();

		var s = new MemoryStream();
		var w = new Writer(s, Rdn.Rdn.Any.Constructor);

		w.WriteNullable(package.Id);
		w.Write(package.Manifest);

		b.Put((package.Address as IBinarySerializable).ToRaw(), s.ToArray());

		Node.Database.Write(b);
	}

 	public static string AddressToDeployment(string packagespath, AutoId resource)
 	{
 		return Path.Join(packagespath, resource.ToString());
 	}

 	public string AddressToReleases(Urn release)
 	{
 		return Path.Join(Node.Settings.Seed.Releases, release.ToString().EscapeFilePath());
 	}

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
				p.Release.Availability.HasFlag(Availability.Delta) && p.Manifest.Parents.Any(i => IsAvailable(i.Id)))
			{
				return p.Manifest.CriticalDependencies.All(i => IsAvailable(i.Id));
			}
			else
				return false;
		}
	}

 	public Package Find(AutoId id)
 	{
 		var p = Packages.Find(i => i.Id == id);
 
 		return p;
 	}

 	public Package Find(Ura address)
 	{
 		var p = Packages.Find(i => i.Address == address);
 
 		return p;
 	}

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
	
	void BuildIncremental(Stream stream, Release complete, IDictionary<string, string> all, Flow flow)
	{
		var rems = new List<string>();
		var upds = new Dictionary<string, string>();
		var pchs = new Dictionary<string, byte[]>();
		var olds = new HashSet<string>();

		//var prev = package.ReplaceHash(previous);

		string ppath;
			
		lock(Node.ResourceHub)
			ppath = complete.Find(Package.Complete).DataPath;

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
			file = from.Id.ToString(); /// we have all incremental packages since last complete one
		}
		else
		{
			dependencies = manifest.CompleteDependencies.ToList();
			file = Package.Complete;
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

			i.AddedDependencies		= manifest.CompleteDependencies.Where(i => !parent.Manifest.CompleteDependencies.Contains(i)).ToArray();
			i.RemovedDependencies	= parent.Manifest.CompleteDependencies.Where(i => !manifest.CompleteDependencies.Contains(i)).ToArray();

			x[ds.ToArray()] = parent.Id.ToString();
		}
		
	 	var p = new Package(this);

		p.Address = address;
		p.Manifest = manifest;
	 	
	 	Packages.Add(p);

		x[completed] = Package.Complete;

		if(instruction != null)
			x[(instruction as IBinarySerializable).ToRaw()] = PackageInstruction.Extension;
					
 		lock(Node.ResourceHub.Lock)
 		{
			var r = Node.ResourceHub.Add(x);

			r.Complete(Availability.Complete|(manifest.Parents.Any() ? Availability.Delta : 0));

			manifest.Urn = r.Address;

			flow.Log?.Report(this, $"Release built: {r.Address}");

 		}

		Save(p);

		return p;
	}

	public void StartDeploy(AutoId address, string packagespath, Flow flow)
	{
		packagespath ??= DeploymentPath;

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
																		throw new PackageException(PackageError.Busy);
				
																	m.Complete = p;

																	break;
																}
																else if(p.Release.Availability.HasFlag(Availability.Delta))
																{	
																	if(p.Activity == null)
																		p.Activity = d;
																	else
																		throw new PackageException(PackageError.Busy);

																	var pp = p.Manifest.Parents.LastOrDefault(i => ExistsRecursively(i.Id));

																	if(pp == null)
																		throw new PackageException(PackageError.ParentPackagesNotFound);

																	m.Deltas.Insert(0, new (p, pp));

																	p = Find(pp.Id);
																}

																Thread.Sleep(10);
															}

															//all.AddRange(s.Select(i => i.Key).AsEnumerable().Reverse());
			
															var deps = new HashSet<AutoId>();

															foreach(var j in m.Complete.Manifest.CompleteDependencies.Where(i => i.Need == DependencyNeed.Critical))
																deps.Add(j.Id);

															foreach(var i in m.Deltas.AsEnumerable().Reverse())
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
									using var cfs = new FileStream(s.Complete.Release.Find(Package.Complete).DataPath, FileMode.Open);
									using var ca = new ZipArchive(cfs, ZipArchiveMode.Read);
									
									var deplpath = AddressToDeployment(packagespath, s.Target.Id); 

									foreach(var e in ca.Entries) /// extract from complete
									{
										var f = Path.Join(deplpath, e.FullName.Replace('/', Path.DirectorySeparatorChar));
								
										Directory.CreateDirectory(Path.GetDirectoryName(f));
										e.ExtractToFile(f, true);
									}

									var prev = s.Complete;

									foreach(var i in s.Deltas)
									{
										using var ifs = new FileStream(i.Key.Release.Find(prev.Id.ToString()).DataPath, FileMode.Open);
										using var z = new ZipArchive(ifs, ZipArchiveMode.Read);
											
										foreach(var e in z.Entries)
										{
											if(e.Name != Package.Removals)
											{
												var f = Path.Join(deplpath, e.FullName.Replace('/', Path.DirectorySeparatorChar));
								
												Directory.CreateDirectory(Path.GetDirectoryName(f));
												e.ExtractToFile(f, true);
											} 
											else
											{
												using var es = e.Open();
												var sr = new StreamReader(es);

												while(!sr.EndOfStream)
												{
													File.Delete(Path.Join(deplpath, sr.ReadLine().Replace('/', Path.DirectorySeparatorChar)));
												}
											}
										}

										prev = i.Key;

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

										foreach(var j in Directory.EnumerateFiles(AddressToDeployment(packagespath, i.Id), "*", SearchOption.AllDirectories).Where(i => Path.GetExtension(i) != PackageInstruction.Extension))
										{
											var fd = Path.Join(AddressToDeployment(packagespath, s.Target.Id), j.Substring(AddressToDeployment(packagespath, i.Id).Length + 1));
											Directory.CreateDirectory(Path.GetDirectoryName(fd));

											File.Copy(j, fd, true);
										}
										//var f = Path.Join(todeployment(s.Target.Resource.Address), e.FullName.Replace('/', Path.DirectorySeparatorChar));
									
									}

									if(s.Complete.Release.Find(PackageInstruction.Extension) != null)
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
			var r = Node.ResourceHub.Get(id);

			p = new Package(this)
				{
					Id		 = id,
					Address	 = r.Address,
					Manifest = r.Data.Get<PackageManifest>(),
				};

			Packages.Add(p);

			Node.ResourceHub.Add(p.Manifest.Urn, id);
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
