using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uccs.Net
{
	public class Package
	{
		public const string		IncrementalFile = "i";
		public const string		CompleteFile = "c";
		public const string		ManifestFile = "m";
		public const string		Removals = ".removals";
		public const string		Renamings = ".renamings"; /// TODO

		public PackageAddress	Address;
		public LocalRelease		Release;
		public LocalResource	Resource;
		PackageHub				Hub;
		Manifest				_Manifest;

		public HistoryRelease	HistoryRelease => History.Releases.First(i => i.Hash.SequenceEqual(Address.Hash));
		public History			History => Hub.Sun.ResourceHub.Find(Address).LastAs<History>();

		public Manifest	Manifest
		{
			get
			{
				if(_Manifest == null)
				{
					_Manifest = new Manifest{};
					
					lock(Hub.Sun.ResourceHub.Lock)
					{
						_Manifest.Read(new BinaryReader(new MemoryStream(Release.ReadFile(ManifestFile))));
					}
				}

				return _Manifest;
			}
		}

		public Package(PackageHub hub, PackageAddress address, LocalResource resource, LocalRelease release)
		{
			if(resource == null || release == null)
				throw new ResourceException();

			Hub = hub;
			Address = address;
			Release = release;
			Resource = resource;
		}

		public Package(PackageHub hub, PackageAddress address, LocalResource resource, LocalRelease release, Manifest manifest)
		{
			if(resource == null || release == null)
				throw new ResourceException();

			Hub = hub;
			Address = address;
			Release = release;
			Resource = resource;
			_Manifest = manifest;
		}

		public override string ToString()
		{
			return Address.ToString();
		}

		public void AddRelease(byte[] hash)
		{
			var h = History == null ? new History {Releases = new()} : new History(History.Raw);

			h.Releases.Add(new HistoryRelease {Hash = hash});
			
			Resource.AddData(DataType.Package, h);
		}
	}
	
	public class PackageHub
	{
		string							ProductsPath;
		public List<Package>			Packages = new();
		public Sun						Sun;
		public List<PackageDownload>	Downloads = new();
		public object					Lock = new object();

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

// 		public void Add(PackageAddress release, Manifest manifest)
// 		{
// 			var s = new MemoryStream();
// 
// 			manifest.Write(new BinaryWriter(s));
// 
// 			Add(release, manifest.GetOrCalcHash(), s.ToArray());
// 		}
// 
// 		public void Add(PackageAddress package, byte[] manifesthash, byte[] manifest)
// 		{
// 			lock(ResourceBase.Lock)
// 			{
// 				var r = ResourceBase.Add(package, manifesthash);
// 
// 				ResourceBase.WriteFile(package, manifesthash, Package.ManifestFile, 0, manifest);
// 
// 				Packages.Add(new Package(this, package, r));
// 			}
// 		}

		public History GetHistory(ResourceAddress resource)
		{
			return Sun.ResourceHub.Find(resource).LastAs<History>();
		}

		IEnumerable<Package> PreviousIncrementals(PackageAddress package, byte[] incrementalminimal)
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

		public Package Get(PackageAddress package)
		{
			var p = Find(package);

			if(p != null)
				return p;

			lock(Sun.ResourceHub.Lock)
			{
				p = new Package(this, package, Sun.ResourceHub.Find(package) ?? Sun.ResourceHub.Add(package), Sun.ResourceHub.Find(package.Hash) ?? Sun.ResourceHub.Add(package.Hash, DataType.Package));
			}

			Packages.Add(p);

			return p;
		}

		public Package Find(ResourceAddress resource)
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
					p = new Package(this, new PackageAddress(resource, h), r, Sun.ResourceHub.Find(h));
	
					Packages.Add(p);
	
					return p;
				}
			}

			return null;
		}

		public Package Find(PackageAddress package)
		{
			var p = Packages.Find(i => i.Address == package);

			if(p != null)
				return p;

			LocalRelease r;

			lock(Sun.ResourceHub.Lock)
			{
				r = Sun.ResourceHub.Find(package.Hash);

				if(r != null)
				{
					p = new Package(this, package, Sun.ResourceHub.Find(package), r);
	
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
					var e = arch.CreateEntry(Package.Removals);
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
				path = Sun.ResourceHub.Find(previous).MapPath(Package.CompleteFile);

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
					pm.Read(new BinaryReader(new MemoryStream(Sun.ResourceHub.Find(previous).ReadFile(Package.ManifestFile))));
				
					var d = new ParentPackage {	Release				= previous,
												AddedDependencies	= m.CompleteDependencies.Where(i => !pm.CompleteDependencies.Contains(i)).ToArray(),
												RemovedDependencies	= pm.CompleteDependencies.Where(i => !m.CompleteDependencies.Contains(i)).ToArray() };
					
					m.Parents = new ParentPackage[] {d};
				}

				var h = Sun.ResourceHub.Zone.Cryptography.HashFile(m.Raw);
 				
				var p = Get(new PackageAddress(resource, h));
				p.AddRelease(h);
 				
				var r = Sun.ResourceHub.Find(h);
				 
 				r.AddFile(Package.ManifestFile, m.Raw);
				r.AddFile(Package.CompleteFile, cstream.ToArray());

				if(istream != null)
					r.AddFile(Package.IncrementalFile, istream.ToArray());
				
				r.Complete(Availability.Complete|(istream != null ? Availability.Incremental : 0));

				workflow.Log?.Report(this, $"Manifest Hash: {h.ToHex()}");
 			}
		}

		public void Unpack(PackageAddress package, bool overwrite = false)
		{
			var p = Find(package);
			var dir = p.Release.Path;

			var pks = new List<KeyValuePair<Package, ParentPackage>>();

			while(!p.Release.Availability.HasFlag(Availability.Complete))
			{
				var a = p.Manifest.Parents.LastOrDefault(i => Find(new PackageAddress(package, i.Release)) != null);

				if(a != null && p.Release.Availability.HasFlag(Availability.Complete) || p.Release.Availability.HasFlag(Availability.Incremental))
				{
					p = Find(new PackageAddress(package, a.Release));
					pks.Add(new (p, a));
				}
				else
					throw new ResourceException("Required packages not found");
			}

			var deps = new List<Dependency>();

			var c = Find(package);

			using(var s = new FileStream(c.Release.MapPath(Package.CompleteFile), FileMode.Open))
			{
				using(var arch = new ZipArchive(s, ZipArchiveMode.Read))
				{
					foreach(var e in arch.Entries)
					{
						var f = Path.Join(AddressToPath(package), e.FullName.Replace('/', Path.DirectorySeparatorChar));
								
						if(!File.Exists(f) || overwrite)
						{
							Directory.CreateDirectory(Path.GetDirectoryName(f));
							e.ExtractToFile(f, true);
						}
					}
				}
			}

			deps.AddRange(c.Manifest.CompleteDependencies);

			foreach(var i in pks.AsEnumerable().Reverse())
			{
				using(var s = new FileStream(i.Key.Release.MapPath(Package.IncrementalFile), FileMode.Open))
				{
					using(var arch = new ZipArchive(s, ZipArchiveMode.Read))
					{
						foreach(var e in arch.Entries)
						{
							if(e.Name != Package.Removals)
							{
								var f = Path.Join(AddressToPath(package), e.FullName.Replace('/', Path.DirectorySeparatorChar));
									
								if(!File.Exists(f) || overwrite)
								{
									Directory.CreateDirectory(Path.GetDirectoryName(f));
									e.ExtractToFile(f, true);
								}
							} 
							else
							{
								using(var es = e.Open())
								{
									var sr = new StreamReader(es);

									while(!sr.EndOfStream)
									{
										File.Delete(Path.Join(ProductsPath, sr.ReadLine()));
									}
								}
							}
						}
					}
				}

				foreach(var j in i.Value.AddedDependencies)
				{
					///Unpack(j.Package);
					deps.Add(j);
				}

				foreach(var j in i.Value.RemovedDependencies)
				{
					deps.Remove(j);
				}
			}

			if(deps.Any())
			{
				foreach(var i in deps)
				{
					Unpack(i.Package);
				}

				//var f = Path.Join(AddressToPath(package), $".{Manifest.Extension}");
				//	
				//if(!File.Exists(f) || overwrite)
				//{
				//	using(var s = File.Create(f))
				//	{
				//		var d = new XonDocument(new XonTextValueSerializator());
				//		d.Nodes.AddRange(deps.Select(i => new Xon(d.Serializator){ Name = i.Type.ToString(), Value = i.Package.ToString() }));
				//		d.Save(new XonTextWriter(s, Encoding.UTF8));
				//	}
				//}
			}
		}

		public void Add(ResourceAddress resource, IEnumerable<string> sources, string dependenciespath, Workflow workflow)
		{
			//var qlatest = Sun.Call(p => p.QueryResource($"{release.APR}/"), workflow);
			//var previos = qlatest.Resources.OrderBy(i => PackageAddress.ParseVesion(i.Resource)).FirstOrDefault();

			var r = Sun.ResourceHub.Find(resource);

			AddRelease(resource, sources, dependenciespath, r?.LastAs<History>().Releases.Last().Hash, workflow);
		}

		public void Install(PackageAddress release, Workflow workflow)
		{
			Task.Run(() =>	{ 
								try
								{
									if(!ExistsRecursively(release))
									{
										var d = Download(release, workflow);
		
										d.Task.Wait(workflow.Cancellation);
									}
					
									Unpack(release);
								}
								catch(OperationCanceledException)
								{
								}
							});
		}

		public PackageDownload Download(PackageAddress package, Workflow workflow)
		{
			var d = Downloads.Find(j => j.Address == package);
				
			if(d != null)
				return d;

			d = new PackageDownload(Sun, package, workflow);
			Downloads.Add(d);
		
			return d;
		}

		public PackageDownloadProgress GetDownloadProgress(PackageAddress package)
		{
			var d = Downloads.Find(i => i.Address == package);

			if(d == null)
				return null;
			
			var s = new PackageDownloadProgress();

			s.Succeeded						 = d.Succeeded;
			s.DependenciesRecursiveCount	 = d.DependenciesRecursiveCount;
			s.DependenciesRecursiveSuccesses = d.DependenciesRecursiveSuccesses;

			lock(Sun.ResourceHub.Lock)
			{
				if(d.Package != null)
				{
					s.CurrentFiles = Sun.ResourceHub.FileDownloads.Where(i => i.Release == d.Package.Release).Select(i => new FileDownloadProgress(i)).ToArray();
				}
	
			}
			
			s.Dependencies = d.Dependencies.Select(i => GetDownloadProgress(i.Address)).Where(i => i != null).ToArray();

			return s;
		}
	}
}
