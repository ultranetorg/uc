using System.IO.Compression;
using System.Text;

namespace Uccs.Rdn
{
	public class PackageHub
	{
		SeedSettings				Settings;
		public List<LocalPackage>	Packages = new();
		public RdnNode					Node;
		public object				Lock = new object();

		public PackageHub(RdnNode sun, SeedSettings settings)
		{
			Node = sun;
			Settings = settings;

			Directory.CreateDirectory(Settings.Packages);
		}

 		public string AddressToDeployment(Ura resource)
 		{
 			return Path.Join(Settings.Packages, ResourceHub.Escape(resource.ToString()));
 		}
 
 		public Ura DeploymentToAddress(string path)
 		{
 			return Ura.Parse(ResourceHub.Unescape(path.Substring(Settings.Packages.Length)));
 		}

 		public string AddressToReleases(Urr release)
 		{
 			return Path.Join(Settings.Releases, ResourceHub.Escape(release.ToString()));
 		}

		IEnumerable<LocalPackage> PreviousIncrementals(Ura package, Ura incrementalminimal)
		{
			return Find(package).Manifest.History	//.TakeWhile(i => !i.SequenceEqual(package.Hash))
													.SkipWhile(i => i != incrementalminimal)
													.Select(i => Find(i))
													.Where(i => i is not null);
		}

		public bool IsReady(Ura package) 
		{
			var p = Find(package);
				
			if(p == null)
				return false;

			if(p.Release == null)
				return false;
	
			lock(Node.ResourceHub.Lock)
			{
				if(	p.Release.Availability.HasFlag(Availability.Complete) || 
					p.Release.Availability.HasFlag(Availability.Incremental) && p.Manifest.Parents.Any(i => IsReady(i.Release)))
				{
					return p.Manifest.CriticalDependencies.All(i => IsReady(i.Package));
				}
				else
					return false;
			}
		}

		public LocalPackage Get(Ura resource)
		{
			var p = Find(resource);

			if(p != null)
				return p;

			lock(Node.ResourceHub.Lock)
			{
				var r = Node.ResourceHub.Find(resource) ?? Node.ResourceHub.Add(resource);
				p = new LocalPackage(this, r);
			}

			Packages.Add(p);

			return p;
		}

 		public LocalPackage Find(Ura resource)
 		{
 			var p = Packages.Find(i => i.Resource.Address == resource);
 
 			if(p != null)
 				return p;
 
 			LocalResource r;
 
 			lock(Node.ResourceHub.Lock)
 			{
 				r = Node.ResourceHub.Find(resource);
 
 				if(r != null)
 				{
 					p = new LocalPackage(this, r);
 	
 					Packages.Add(p);
 	
 					return p;
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

		public bool ExistsRecursively(Ura release)
		{
			var p = Find(release);

			if(p?.Manifest == null)
				return false;

			foreach(var i in p.Manifest.CompleteDependencies)
			{
				if(i.Type == DependencyType.Critical && !ExistsRecursively(i.Package))
					return false;
			}

			return true;
		}

		public void Build(Stream stream, IDictionary<string, string> files, IEnumerable<string> removals, Flow workflow)
		{
			using(var arch = new ZipArchive(stream, ZipArchiveMode.Create, true))
			{
				foreach(var f in files)
				{
					arch.CreateEntryFromFile(f.Key, f.Value);
					workflow.Log?.Report(this, "Packed", f.Value);
				}

				if(removals.Any())
				{
					var e = arch.CreateEntry(LocalPackage.Removals);
					var f = string.Join('\n', removals);
						
					using(var s = e.Open())
					{
						s.Write(Encoding.UTF8.GetBytes(f));
					}
				}
			}
		}
		
		public void BuildIncremental(Stream stream, Ura package, Ura previous, IDictionary<string, string> files, Flow workflow)
		{
			var rems = new List<string>();
			var incs = new Dictionary<string, string>();
			var olds = new List<string>();

			//var prev = package.ReplaceHash(previous);

			string path;
				
			lock(Node.ResourceHub)
				path = Find(previous).Release.Find(LocalPackage.CompleteFile).LocalPath;

			using(var s = new FileStream(path, FileMode.Open))
			{
				using(var arch = new ZipArchive(s, ZipArchiveMode.Read))
				{
					foreach(var e in arch.Entries)
					{
						olds.Add(e.FullName);

						var f = files.FirstOrDefault(i => i.Value == e.FullName);
					
						if(f.Key != null) /// new package contains a file from the old one
						{
							bool changed = e.Length != new FileInfo(f.Key).Length;

							if(!changed)
							{
								var a = e.Open();
								var b =	File.OpenRead(f.Key);

								var abuffer = new byte[e.Length];
								var bbuffer = new byte[e.Length];

								var n = a.Read(abuffer, 0, (int)e.Length);

								while(n < e.Length)
									n += a.Read(abuffer, n, (int)e.Length - n);

								var m = b.Read(bbuffer, 0, (int)e.Length);

								while(m < e.Length)
									m += b.Read(bbuffer, m, (int)e.Length - m);
																
								changed = !abuffer.SequenceEqual(bbuffer);

								a.Close();
								b.Close();
							}

							if(changed)
							{
								incs.Add(f.Key, f.Value);
								workflow?.Log?.Report(this, "Updated", f.Value);
							}
						}
						else /// a file is removed in the new package
						{
							rems.Add(e.FullName);
							workflow?.Log?.Report(this, "Removed", e.FullName);
						}
					}
				}
			}

			foreach(var f in files)
			{
				if(!olds.Contains(f.Value)) /// a completely new file
				{
					incs.Add(f.Key, f.Value);
					workflow?.Log?.Report(this, "New", f.Value);
				}
			}
			
			Build(stream, incs, rems, workflow);
		}

		public void DetermineDelta(Ura package, Manifest manifest, out bool canincrement, out List<Dependency> dependencies)
		{
			lock(Node.ResourceHub.Lock)
			{
				var from = manifest.Parents.LastOrDefault(i => IsReady(i.Release));
		
				if(from != null)
				{
					var deps = Find(from.Release).Manifest.CompleteDependencies.ToList();

					deps.AddRange(from.AddedDependencies);
					deps.RemoveAll(i => from.RemovedDependencies.Contains(i));
								
					deps.AddRange(manifest.CompleteDependencies.Where(i => !deps.Contains(i)));
					deps.RemoveAll(i => !manifest.CompleteDependencies.Contains(i));
					
					dependencies = deps;
					canincrement = true; /// we have all incremental packages since last complete one
				}
				else
				{
					dependencies = manifest.CompleteDependencies.ToList();
					canincrement = false;
				}
			}
		}

		public Urr AddRelease(Ura resource, IEnumerable<string> sources, string dependenciespath, Ura[] history, Ura previous, ReleaseAddressCreator addresscreator, Flow workflow)
		{
			var cstream = new MemoryStream();
			var istream = (MemoryStream)null;

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

			Build(cstream, files, [], workflow);

			if(previous != null)
			{
				istream = new MemoryStream();
				BuildIncremental(istream, resource, previous, files, workflow);
			}
			
			var m = File.Exists(dependenciespath) ? Manifest.LoadCompleteDependencies(dependenciespath)
												  : new Manifest{CompleteDependencies = new Dependency[0]};

			///Add(release, m);
			
 			lock(Node.ResourceHub.Lock)
 			{
				m.CompleteHash		= Node.ResourceHub.Zone.Cryptography.HashFile(cstream.ToArray());
				m.IncrementalHash	= istream != null ? Node.ResourceHub.Zone.Cryptography.HashFile(istream.ToArray()) : null;

				if(previous != null) /// a single parent supported only
				{
					var pm = new Manifest();
					pm.Read(new BinaryReader(new MemoryStream(Find(previous).Release.Find(LocalPackage.ManifestFile).Read())));
				
					var d = new ParentPackage {	Release				= previous,
												AddedDependencies	= m.CompleteDependencies.Where(i => !pm.CompleteDependencies.Contains(i)).ToArray(),
												RemovedDependencies	= pm.CompleteDependencies.Where(i => !m.CompleteDependencies.Contains(i)).ToArray() };
					
					m.Parents = [d];
					m.History = history.Append(previous).ToArray();
				}

				var h = Node.ResourceHub.Zone.Cryptography.HashFile(m.Raw);

				var a = addresscreator.Create(Node.Mcv, h);
				var r = Node.ResourceHub.Add(a, DataType.Package);
				 
 				r.AddCompleted(LocalPackage.ManifestFile, Path.Join(AddressToReleases(a), LocalPackage.ManifestFile), m.Raw);
				r.AddCompleted(LocalPackage.CompleteFile, Path.Join(AddressToReleases(a), LocalPackage.CompleteFile), cstream.ToArray());

				if(istream != null)
					r.AddCompleted(LocalPackage.IncrementalFile, Path.Join(AddressToReleases(a), LocalPackage.IncrementalFile), istream.ToArray());

				r.Complete(Availability.Complete|(istream != null ? Availability.Incremental : 0));
 				
				var p = Get(resource);
				p.Resource.AddData(DataType.Package, a);

				workflow.Log?.Report(this, $"Address: {a}");

				return a;
 			}
		}

		public Urr AddRelease(Ura resource, IEnumerable<string> sources, string dependenciespath, ReleaseAddressCreator addresscreator, Flow workflow)
		{
			var r = Node.ResourceHub.Find(resource);
			var m = new Manifest();
		
			if(r != null)
			{
				var c = Node.ResourceHub.Find(r.LastAs<Urr>());
				m.Read(new BinaryReader(new MemoryStream(c.Find("m").Read())));
			}
		
			 return AddRelease(resource, sources, dependenciespath, m.History, m.History?.LastOrDefault(), addresscreator, workflow);
		}

		public Deployment Deploy(Ura package, Flow workflow)
		{
			var	d = new Deployment();

			void collect(LocalPackage parent, Ura address)
			{
				var m = new DeploymentMerge{Target = Find(address)};
				d.Merges.Add(m);

				var p = m.Target;

				while(true)
				{
					if(p.Release.Availability.HasFlag(Availability.Complete))
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

						var pp = p.Manifest.Parents.LastOrDefault(i => ExistsRecursively(i.Release));

						if(pp == null)
							throw new ResourceException(ResourceError.RequiredPackagesNotFound);

						m.Incrementals.Insert(0, new (p, pp));

						p = Find(pp.Release);
					}
				}

				//all.AddRange(s.Select(i => i.Key).AsEnumerable().Reverse());
				
				var deps = new HashSet<Ura>();

				foreach(var j in m.Complete.Manifest.CompleteDependencies)
					deps.Add(j.Package);

				foreach(var i in m.Incrementals.AsEnumerable().Reverse())
				{
					foreach(var j in i.Value.AddedDependencies)
						deps.Add(j.Package);
		
					foreach(var j in i.Value.RemovedDependencies)
						deps.Remove(j.Package);
				}
	
				foreach(var i in deps)
					collect(p, i);
			}

			lock(Lock)
				collect(null, package);

			Task.Run(() =>	{ 
								foreach(var s in d.Merges.AsEnumerable().Reverse())
								{
									using(var fs = new FileStream(s.Complete.Release.Find(LocalPackage.CompleteFile).LocalPath, FileMode.Open))
									{
										using(var a = new ZipArchive(fs, ZipArchiveMode.Read))
										{
											foreach(var e in a.Entries)
											{
												var f = Path.Join(AddressToDeployment(s.Target.Resource.Address), e.FullName.Replace('/', Path.DirectorySeparatorChar));
									
												Directory.CreateDirectory(Path.GetDirectoryName(f));
												e.ExtractToFile(f, true);
											}
										}
									}

									s.Complete.Activity = null;

									foreach(var i in s.Incrementals)
									{
										using(var fs = new FileStream(i.Key.Release.Find(LocalPackage.IncrementalFile).LocalPath, FileMode.Open))
										{
											using(var z = new ZipArchive(fs, ZipArchiveMode.Read))
											{
												foreach(var e in z.Entries)
												{
													if(e.Name != LocalPackage.Removals)
													{
														var f = Path.Join(AddressToDeployment(s.Target.Resource.Address), e.FullName.Replace('/', Path.DirectorySeparatorChar));
									
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
																File.Delete(Path.Join(AddressToDeployment(s.Target.Resource.Address), sr.ReadLine().Replace('/', Path.DirectorySeparatorChar)));
															}
														}
													}
												}
											}
										}

										i.Key.Activity = null;
									}
								}
							});

			return d;
		}

		public PackageDownload Download(Ura package, Flow workflow)
		{
			var p = Get(package);

			if(p.Activity is PackageDownload d)
				return d;
			else if(p.Activity != null)
				throw new ResourceException(ResourceError.Busy);
				
			d = new PackageDownload(Node, p, workflow);

			return d;
		}

		public void Install(Ura package, Flow workflow)
		{
			bool e; 

			lock(Lock)
				e = ExistsRecursively(package);

			if(!e)
			{
				PackageDownload d;

				lock(Lock)
					d = Download(package, workflow);

				d.Task.Wait(workflow.Cancellation);
			}
				
			Deploy(package, workflow);
		}
	}
}
