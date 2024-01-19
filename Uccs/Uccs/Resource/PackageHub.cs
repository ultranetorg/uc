using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uccs.Net
{

	public class PackageHub
	{
		string									ProductsPath;
		public List<LocalPackage>				Packages = new();
		public Sun								Sun;
		public object							Lock = new object();
		//List<PackageInstallation>				Installations = new ();

		public PackageHub(Sun sun, string productspath)
		{
			Sun = sun;
			ProductsPath = productspath;

			Directory.CreateDirectory(ProductsPath);
		}

		public string AddressToPath(PackageAddress release)
		{
			return Path.Join(ProductsPath, $"{release.APR}{Path.DirectorySeparatorChar}{release.Hash.ToHex()}");
		}

		public PackageAddress PathToAddress(string path)
		{
			path = path.Substring(ProductsPath.Length);

			var i = path.IndexOf(Path.DirectorySeparatorChar);
			var apr = path.Substring(0, i).Split(' ');
			var b = path.Substring(i + 1);
			var h = b.Substring(0, b.IndexOf(Path.DirectorySeparatorChar));

			return new PackageAddress(apr[0], apr[1], apr[2], h.FromHex());
		}

		public History GetHistory(ResourceAddress resource)
		{
			return Sun.ResourceHub.Find(resource).LastAs<History>();
		}

		IEnumerable<LocalPackage> PreviousIncrementals(PackageAddress package, byte[] incrementalminimal)
		{
			return Find(package).History.Releases	.TakeWhile(i => !i.Hash.SequenceEqual(package.Hash))
													.SkipWhile(i => !i.Hash.SequenceEqual(incrementalminimal))
													.Select(i => Find(package.ReplaceHash(i.Hash)))
													.Where(i => i is not null);
		}

		public bool IsReady(PackageAddress package)
		{
			var p = Find(package);
	
			if(p == null)
				return false;
	
			lock(Sun.ResourceHub.Lock)
			{
				if(	p.Release.Availability.HasFlag(Availability.Complete) || 
					p.Release.Availability.HasFlag(Availability.Incremental) && p.Manifest.Parents.Any(i => IsReady(new PackageAddress(package, i.Release))))
				{
					return p.Manifest.CriticalDependencies.All(i => IsReady(i.Package));
				}
				else
					return false;
			}
		}

		public LocalPackage Get(PackageAddress package)
		{
			var p = Find(package);

			if(p != null)
				return p;

			lock(Sun.ResourceHub.Lock)
			{
				p = new LocalPackage(this, package, Sun.ResourceHub.Find(package) ?? Sun.ResourceHub.Add(package), Sun.ResourceHub.Find(package.Hash) ?? Sun.ResourceHub.Add(package.Hash, DataType.Package));
			}

			Packages.Add(p);

			return p;
		}

 		public LocalPackage Find(ResourceAddress resource)
 		{
 			var p = Packages.Find(i => i.Address == resource);
 
 			if(p != null)
 				return p;
 
 			LocalResource r;
 
 			lock(Sun.ResourceHub.Lock)
 			{
 				r = Sun.ResourceHub.Find(resource);
 
 				if(r != null)
 				{
 					var h = r.LastAs<History>().Releases.Last().Hash;
 					p = new LocalPackage(this, new PackageAddress(resource, h), r, Sun.ResourceHub.Find(h));
 	
 					Packages.Add(p);
 	
 					return p;
 				}
 			}
 
 			return null;
 		}

		public LocalPackage Find(PackageAddress package)
		{
			var p = Packages.Find(i => i.Address == package);

			if(p != null)
				return p;

			lock(Sun.ResourceHub.Lock)
			{
				var rs = Sun.ResourceHub.Find(package);
				var rl = Sun.ResourceHub.Find(package.Hash);

				if(rs != null && rl != null)
				{
					p = new LocalPackage(this, package, rs, rl);

					Packages.Add(p);

					return p;
				}
			}

			return null;
		}

		public bool ExistsRecursively(PackageAddress release)
		{
			var p = Find(release);

			if(p == null)
				return false;

			foreach(var i in p.Manifest.CompleteDependencies)
			{
				if(i.Type == DependencyType.Critical && !ExistsRecursively(i.Package))
					return false;
			}

			return true;
		}

		public void Build(Stream stream, IDictionary<string, string> files, IEnumerable<string> removals, Workflow workflow)
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
		
		public void BuildIncremental(Stream stream, ResourceAddress package, byte[] previous, IDictionary<string, string> files, Workflow workflow)
		{
			var rems = new List<string>();
			var incs = new Dictionary<string, string>();
			var olds = new List<string>();

			//var prev = package.ReplaceHash(previous);

			string path;
				
			lock(Sun.ResourceHub)
				path = Sun.ResourceHub.Find(previous).MapPath(LocalPackage.CompleteFile);

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

		public void DetermineDelta(PackageAddress package, Manifest manifest, out bool canincrement, out List<Dependency> dependencies)
		{
			lock(Sun.ResourceHub.Lock)
			{
				var from = manifest.Parents.LastOrDefault(i => IsReady(new PackageAddress(package, i.Release)));
		
				if(from != null)
				{
					var deps = Find(new PackageAddress(package, from.Release)).Manifest.CompleteDependencies.ToList();

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

		public void AddRelease(ResourceAddress resource, IEnumerable<string> sources, string dependenciespath, byte[] previous, Workflow workflow)
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

			Build(cstream, files, new string[]{}, workflow);

			if(previous != null)
			{
				istream = new MemoryStream();
				BuildIncremental(istream, resource, previous, files, workflow);
			}
			
			var m = File.Exists(dependenciespath) ? Manifest.LoadCompleteDependencies(dependenciespath)
												  : new Manifest{CompleteDependencies = new Dependency[0]};

			///Add(release, m);
			
 			lock(Sun.ResourceHub.Lock)
 			{
				m.CompleteHash		= Sun.ResourceHub.Zone.Cryptography.HashFile(cstream.ToArray());
				m.IncrementalHash	= istream != null ? Sun.ResourceHub.Zone.Cryptography.HashFile(istream.ToArray()) : null;

				if(previous != null)
				{
					var pm = new Manifest();
					pm.Read(new BinaryReader(new MemoryStream(Sun.ResourceHub.Find(previous).ReadFile(LocalPackage.ManifestFile))));
				
					var d = new ParentPackage {	Release				= previous,
												AddedDependencies	= m.CompleteDependencies.Where(i => !pm.CompleteDependencies.Contains(i)).ToArray(),
												RemovedDependencies	= pm.CompleteDependencies.Where(i => !m.CompleteDependencies.Contains(i)).ToArray() };
					
					m.Parents = new ParentPackage[] {d};
				}

				var h = Sun.ResourceHub.Zone.Cryptography.HashFile(m.Raw);
 				
				var p = Get(new PackageAddress(resource, h));
				p.AddRelease(h);
 				
				var r = Sun.ResourceHub.Find(h);
				 
 				r.AddCompleted(LocalPackage.ManifestFile, m.Raw);
				r.AddCompleted(LocalPackage.CompleteFile, cstream.ToArray());

				if(istream != null)
					r.AddCompleted(LocalPackage.IncrementalFile, istream.ToArray());
				
				r.Complete(Availability.Complete|(istream != null ? Availability.Incremental : 0));

				workflow.Log?.Report(this, $"Manifest Hash: {h.ToHex()}");
 			}
		}

		public void Deploy(PackageAddress package, Workflow workflow)
		{
			//if(Installations.Any())
			//	throw new ResourceException(ResourceError.Busy);
			var	all = new List<LocalPackage>();

			void collect(PackageAddress address)
			{
				var t = Find(address);
				var p = t;

				var sequence = new List<KeyValuePair<LocalPackage, ParentPackage>>();	

				while(true)
				{
					if(p.Release.Availability.HasFlag(Availability.Complete))
					{
						if(p.Activity == null)
							p.Activity = new PackageDeployment {Target = t, Complete = p};
						else
							throw new ResourceException(ResourceError.Busy);
					
						sequence.Add(new (p, null));

						break;
					}
					else if(p.Release.Availability.HasFlag(Availability.Incremental))
					{	
						if(p.Activity == null)
							p.Activity = new PackageDeployment {Target = t, Incremental = p};
						else
							throw new ResourceException(ResourceError.Busy);

						var pp = p.Manifest.Parents.LastOrDefault(i => ExistsRecursively(new (address, i.Release)));

						if(pp == null)
							throw new ResourceException(ResourceError.RequiredPackagesNotFound);

						sequence.Add(new (p, pp));

						p = Find(new (address, pp.Release));
					}
				}

				//p.Activity = new PackageInstallation {Target = t, Complete = p};
				//Installations.Add(new PackageInstallation{Target = t, Complete = p});

				var deps = new HashSet<PackageAddress>();
				
				all.AddRange(sequence.Select(i => i.Key).AsEnumerable().Reverse());

				foreach(var i in sequence.AsEnumerable().Reverse())
				{
					//Installations.Add(new PackageInstallation{Target = t, Incremental = i.Key});
	
					if(i.Value == null)
					{
						foreach(var j in i.Key.Manifest.CompleteDependencies)
							deps.Add(j.Package);
					} 
					else
					{
						foreach(var j in i.Value.AddedDependencies)
							deps.Add(j.Package);
		
						foreach(var j in i.Value.RemovedDependencies)
							deps.Remove(j.Package);
					}
				}
	
				foreach(var i in deps)
					collect(i);
			}

			lock(Lock)
				collect(package);

			foreach(var i in all)
			{
				var pi = i.Activity as PackageDeployment;

				using(var s = new FileStream((pi.Complete ?? pi.Incremental).Release.MapPath(pi.Complete != null ? LocalPackage.CompleteFile : LocalPackage.IncrementalFile), FileMode.Open))
				{
					using(var a = new ZipArchive(s, ZipArchiveMode.Read))
					{
						foreach(var e in a.Entries)
						{
							if(pi.Complete != null)
							{
								var f = Path.Join(AddressToPath(pi.Target.Address), e.FullName.Replace('/', Path.DirectorySeparatorChar));
									
								Directory.CreateDirectory(Path.GetDirectoryName(f));
								e.ExtractToFile(f, true);
							} 
							else
							{
								if(e.Name != LocalPackage.Removals)
								{
									var f = Path.Join(AddressToPath(pi.Target.Address), e.FullName.Replace('/', Path.DirectorySeparatorChar));
									
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
											File.Delete(Path.Join(AddressToPath(pi.Target.Address), sr.ReadLine().Replace('/', Path.DirectorySeparatorChar)));
										}
									}
								}
							}
						}
					}
				}

				i.Activity = null;
			}

			lock(Lock)
			{
				all.Clear();
			}
		}

		public void Add(ResourceAddress resource, IEnumerable<string> sources, string dependenciespath, Workflow workflow)
		{
			//var qlatest = Sun.Call(p => p.QueryResource($"{release.APR}/"), workflow);
			//var previos = qlatest.Resources.OrderBy(i => PackageAddress.ParseVesion(i.Resource)).FirstOrDefault();

			var r = Sun.ResourceHub.Find(resource);

			AddRelease(resource, sources, dependenciespath, r?.LastAs<History>().Releases.Last().Hash, workflow);
		}

		public PackageDownload Download(PackageAddress package, Workflow workflow)
		{
			var p = Get(package);

			if(p.Activity is PackageDownload d)
				return d;
			else if(p.Activity != null)
				throw new ResourceException(ResourceError.Busy);
				
			d = new PackageDownload(Sun, p, workflow);
			
			p.Activity = d;

			return d;
		}

		public void Install(PackageAddress package, Workflow workflow)
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
