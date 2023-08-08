using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.RLP;

namespace Uccs.Net
{
	public class Package
	{
		public const string		IncrementalFile = "i";
		public const string		CompleteFile = "c";
		public const string		ManifestFile = "m";
		public const string		Removals = ".removals";
		public const string		DependenciesExt = "dependencies";
		public const string		Renamings = ".renamings"; /// TODO

		public PackageAddress	Address;
		public ReleaseBaseItem	Release;
		PackageBase				PackageBase;
		Manifest				_Manifest;

		public Manifest	Manifest
		{
			get
			{
				if(_Manifest == null)
				{
					_Manifest = new Manifest(PackageBase.Core.Zone){Address = Address};
					
					lock(PackageBase.Resources.Lock)
					{
						_Manifest.Read(new BinaryReader(new MemoryStream(PackageBase.Resources.ReadFile(Release.Address, Release.Hash, ManifestFile))));
					}
				}

				return _Manifest;
			}
		}

		public Package(PackageBase packbase, PackageAddress address, ReleaseBaseItem release)
		{
			Address = address;
			PackageBase = packbase;
			Release = release;
		}

		public Package(PackageBase packebase, PackageAddress address, ReleaseBaseItem release, Manifest manifest)
		{
			PackageBase = packebase;
			Address = address;
			Release = release;
			_Manifest = manifest;
		}

		public override string ToString()
		{
			return Address.ToString();
		}
	}
	
	public class PackageBase
	{
		public ResourceBase				Resources;
		string							ProductsPath;
		List<Package>					Packages = new();
		public Core						Core;
		List<PackageDownload>			Downloads = new();
		public object					Lock = new object();

		public PackageBase(Core core, ResourceBase filebase, string productspath)
		{
			Core = core;
			Resources = filebase;
			ProductsPath = productspath;

			Directory.CreateDirectory(ProductsPath);
		}

		public string AddressToPath(PackageAddress release)
		{
			return Path.Join(ProductsPath, @$"{release.Author} {release.Product} {release.Realization}{Path.DirectorySeparatorChar}{release.Version}");
		}

		public PackageAddress PathToAddress(string path)
		{
			path = path.Substring(ProductsPath.Length);

			var i = path.IndexOf(Path.DirectorySeparatorChar);
			var apr = path.Substring(0, i).Split(' ');
			var v = path.Substring(i + 1);
			v = v.Substring(0, v.IndexOf(Path.DirectorySeparatorChar));

			return new PackageAddress(apr[0], apr[1], apr[2], Version.Parse(v));
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

		public Package Find(PackageAddress package)
		{
			var p = Packages.Find(i => i.Address == package);

			if(p != null)
				return p;

			ReleaseBaseItem r;

			//var rp = Core.Call(Role.Base, p => p.FindResource(package), Core.Workflow);

			//AddressToPath(package);

			lock(Resources.Lock)
				r = Resources.Find(package, null);

			if(r != null)
			{
				p = new Package(this, package, r);
	
				Packages.Add(p);
	
				return p;
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
				if(i.Type == DependencyType.Critical && !ExistsRecursively(i.Release))
					return false;
			}

			return true;
		}

		public void Build(Stream stream, ResourceAddress release, IDictionary<string, string> files, IEnumerable<string> removals, Workflow workflow)
		{
			using(var arch = new ZipArchive(stream, ZipArchiveMode.Create, true))
			{
				foreach(var f in files)
				{
					arch.CreateEntryFromFile(f.Key, f.Value);
					workflow?.Log?.Report(this, "Packed", f.Value);
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
		
		public void BuildIncremental(Stream stream, PackageAddress release, Version previous, IDictionary<string, string> files, out Version minimal, Workflow workflow)
		{
			var rems = new List<string>();
			var incs = new Dictionary<string, string>();
			var olds = new List<string>();

			//var r = ResourceBase.GetDirectory(release, null, true);

			//r = r.Substring(0, r.LastIndexOf(' ') + 1);
			var prev = new PackageAddress(release.Author, release.Product, release.Realization, previous);

			using(var s = new FileStream(Resources.AddressToPath(prev, null, Package.CompleteFile), FileMode.Open))
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

			minimal = previous; /// TODO: determine really minimal
			
			Build(stream, release, incs, rems, workflow);
		}

		public void DetermineDelta(IEnumerable<PackageAddress> history, Manifest manifest, byte[] hash, out bool canincrement, out List<Dependency> dependencies)
		{
			dependencies = new();

			//var dir = ResourceBase.AddressToPath(manifest.Address, hash);
			//
			//if(Directory.Exists(dir))
			//{
				var c = Resources.Releases	.Where(i => i.Address.Author == manifest.Address.Author && i.Address.ToString().StartsWith(manifest.Address.APR))
											.Select(i => PackageAddress.ParseVesion(i.Address.Resource))
											.OrderBy(i => i)
											.TakeWhile(i => i < manifest.Address.Version) 
											.FirstOrDefault();	/// find last complete package
				if(c != null) 
				{
					var need = history.SkipWhile(i => i.Version <= c).TakeWhile(i => i.Version < manifest.Address.Version);
					
					lock(Resources.Lock)
						if(need.All(i => Resources.Exists(i, null, Package.IncrementalFile)))
						{
							foreach(var i in need)
							{
								var r = Find(i);

								dependencies.AddRange(r.Manifest.AddedDependencies);
								dependencies.RemoveAll(j => r.Manifest.RemovedDependencies.Contains(j));
							}
						
							dependencies.AddRange(manifest.AddedDependencies);
							dependencies.RemoveAll(j => manifest.RemovedDependencies.Contains(j));
							canincrement = true; /// we have all incremental packages since last complete one

							return;
						}
				}
			//}

// 			foreach(var i in history.TakeWhile(i => i.Release.Version <= release.Version))
// 			{
// 				var m = GetManifest(i.Release);
// 
// 				dependencies.AddRange(m.AddedDependencies);
// 				dependencies.RemoveAll(j => m.RemovedDependencies.Contains(j));
// 			}

			dependencies = manifest.CompleteDependencies.ToList();
			canincrement = false;
		}

		public void AddRelease(PackageAddress package, Version latestdeclared, IEnumerable<string> sources, string dependsdirectory, Workflow workflow)
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

			var minimal = Version.Zero;

			Build(cstream, package, files, new string[]{}, workflow);

			if(latestdeclared != null)
			{
				istream = new MemoryStream();
				BuildIncremental(istream, package, latestdeclared, files, out minimal, workflow);
			}

			var vs = Directory.EnumerateFiles(dependsdirectory, $"*.{Package.DependenciesExt}").Select(i => Uccs.Version.Parse(Path.GetFileNameWithoutExtension(i))).Where(i => i < package.Version).OrderBy(i => i);

			IEnumerable<Dependency> acd = null;
			IEnumerable<Dependency> rcd = null;

			var f = Path.Join(dependsdirectory, $"{package.Version.ABC}.{Package.DependenciesExt}");
			
			var deps = File.Exists(f) ? new XonDocument(File.ReadAllText(f)).Nodes.Select(i => Dependency.From(i))
									  : new Dependency[]{};

			if(vs.Any())
			{
				var lastdeps = new XonDocument(File.ReadAllText(Path.Join(dependsdirectory, $"{vs.Last()}.{Package.DependenciesExt}"))).Nodes.Select(i => Dependency.From(i));
		
				acd = deps.Where(i => !lastdeps.Contains(i));
				rcd = lastdeps.Where(i => !deps.Contains(i));
			}
			else
				acd = deps;

			var m = new Manifest(	Resources.Zone,
									Resources.Zone.Cryptography.HashFile(cstream.ToArray()),
									cstream.Length,
									deps,
									istream != null ? Resources.Zone.Cryptography.HashFile(istream.ToArray()) : null,
									istream != null ? istream.Length : 0,
									minimal,
									acd,
									rcd);

			///Add(release, m);
			
 			lock(Resources.Lock)
 			{
				var h = Resources.Zone.Cryptography.HashFile(m.Bytes);
 				
				var r = Resources.Add(package, h);
				 
 				Resources.WriteFile(package, h, Package.ManifestFile, 0, m.Bytes);
				Resources.WriteFile(package, h, Package.CompleteFile, 0, cstream.ToArray());

				if(istream != null)
				{
					Resources.WriteFile(package, h, Package.IncrementalFile, 0, istream.ToArray());
				}
 
 				Packages.Add(new Package(this, package, r));

				Resources.SetLatest(package, h);
 			}
		}

		public void Unpack(PackageAddress release, bool overwrite = false)
		{
			var dir = Resources.AddressToPath(release, null);

			var c = Directory.EnumerateFiles(dir, $"*.{Package.CompleteFile}")	.Select(i => Version.Parse(Path.GetFileNameWithoutExtension(i)))
																		.OrderByDescending(i => i)
																		.SkipWhile(i => i > release.Version) /// skip younger
																		.FirstOrDefault();	/// find last available complete package
			if(c != null) 
			{
				var incs = Directory.EnumerateFiles(dir, $"*.{Package.IncrementalFile}")	.Select(i => Version.Parse(Path.GetFileNameWithoutExtension(i)))
																				.OrderBy(i => i)
																				.SkipWhile(i => i <= c)
																				.TakeWhile(i => i <= release.Version); /// take all incremetals before complete
				
				var deps = new List<Dependency>();

				void cunzip(Version v)
				{
					var r = new PackageAddress(release.Author, release.Product, release.Realization, v);

					using(var s = new FileStream(Resources.AddressToPath(r, null, Package.CompleteFile), FileMode.Open))
					{
						using(var arch = new ZipArchive(s, ZipArchiveMode.Read))
						{
							foreach(var e in arch.Entries)
							{
								var f = Path.Join(AddressToPath(release), e.FullName.Replace('/', Path.DirectorySeparatorChar));
								
								if(!File.Exists(f) || overwrite)
								{
									Directory.CreateDirectory(Path.GetDirectoryName(f));
									e.ExtractToFile(f, true);
								}
							}
						}
					}

					var m = Find(r);

					foreach(var i in m.Manifest.CompleteDependencies)
					{
						Unpack(i.Release);
					}

					deps.AddRange(m.Manifest.CompleteDependencies);
				}

				cunzip(c);

				void iunzip(Version v)
				{
					var r = new PackageAddress(release.Author, release.Product, release.Realization, v);

					using(var s = new FileStream(Resources.AddressToPath(r, null, Package.IncrementalFile), FileMode.Open))
					{
						using(var arch = new ZipArchive(s, ZipArchiveMode.Read))
						{
							foreach(var e in arch.Entries)
							{
								if(e.Name != Package.Removals)
								{
									var f = Path.Join(AddressToPath(release), e.FullName.Replace('/', Path.DirectorySeparatorChar));
									
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

					var m = Find(r);

					foreach(var i in m.Manifest.AddedDependencies)
					{
						Unpack(i.Release);
						deps.Add(i);
					}

					foreach(var i in m.Manifest.RemovedDependencies)
					{
						deps.Remove(i);
					}
				}

				foreach(var i in incs)
				{
					iunzip(i);
				}

				if(deps.Any())
				{
					var f = Path.Join(AddressToPath(release), $".{Package.DependenciesExt}");
					
					if(!File.Exists(f) || overwrite)
					{
						using(var s = File.Create(f))
						{
							var d = new XonDocument(new XonTextValueSerializator());
							d.Nodes.AddRange(deps.Select(i => new Xon(d.Serializator){ Name = i.Type.ToString(), Value = i.Release.ToString() }));
							d.Save(new XonTextWriter(s, Encoding.UTF8));
						}
					}
				}
			}
		}

		public ReleaseStatus GetStatus(PackageAddress package, int limit)
		{
			var s = new ReleaseStatus();

			s.ExistsRecursively = ExistsRecursively(package);

			if(s.ExistsRecursively)
			{
				s.Manifest = Find(package).Manifest;
			}
			else
			{
				var p = Downloads.Find(i => i.Address == package);

				if(p != null)
				{
					var r = Resources.Downloads.Find(i => i.Resource == package);

					s.Download = new () {	File					= r.File,
											Length					= r.Length,
											CompletedLength			= r.CompletedLength,
											DependenciesRecursive	= p.DependenciesRecursive.Select(i => new DownloadReport.Dependency {Release = i.Address, Exists = Find(new PackageAddress(i.Address)) != null}).ToArray(),
											Hubs					= r.Hubs.Take(limit).Select(i => new DownloadReport.Hub { Account = i.Account, Seeds = i.Seeds.Take(limit).Select(i => i.IP), Status = i.Status }).ToArray(),
											Seeds					= r.Seeds.Take(limit).Select(i => new DownloadReport.Seed { IP = i.IP, Succeses = i.Succeses, Failures = i.Failures }).ToArray() };
				}
			}

			return s;
		}

		public void Add(PackageAddress release, IEnumerable<string> sources, string dependsdirectory, Workflow workflow)
		{
			var qlatest = Core.Call(Role.Base, p => p.QueryResource($"{release.APR}/"), workflow);
			var previos = qlatest.Resources.OrderBy(i => PackageAddress.ParseVesion(i.Resource)).FirstOrDefault();

			AddRelease(release, previos != null ? PackageAddress.ParseVesion(previos.Resource) : null, sources, dependsdirectory, workflow);
		}

		public void Install(PackageAddress release, Workflow workflow)
		{
			Task.Run(() =>	{ 
								try
								{
									if(!ExistsRecursively(release))
									{
										var d = Download(release, workflow);
		
										d.Task.Wait(workflow.Cancellation.Token);
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
				
			d = new PackageDownload(package);
			Downloads.Add(d);

			d.Task = Task.Run(() =>	{
										byte[] h = null;
										IEnumerable<ResourceAddress> hst = null;

										while(!workflow.IsAborted)
										{
											try
											{
												hst = Core.Call(Role.Base, c => c.QueryResource(package.ToString()), workflow).Resources;
												h = Core.Call(Role.Base, c => c.FindResource(hst.First()), workflow).Resource.Data;
												break;
											}
											catch(RdcEntityException)
											{
												Thread.Sleep(100);
											}
										}

										lock(Lock)
											lock(Core.Resources.Lock)
											{
												d.Package = Find(package);
											
												if(d.Package != null)
												{
													if(d.Package.Release.Hash.SequenceEqual(h))
														goto done;
													else
														d.Package.Release = Core.Resources.Add(package, h); /// update to the latest
												} 
												else
												{	
													d.Package = new Package(this, package, Core.Resources.Add(package, h));
													Packages.Add(d.Package);
												}
											}
	 									
										Core.Resources.GetFile(package, h, Package.ManifestFile, h, workflow);

										bool incrementable;

										lock(Lock)
										{
											//d.Package = Find(package);
	 
	 										//if(!Core.Zone.Cryptography.HashFile(d.Package.Manifest.Bytes).SequenceEqual(h))
	 										//	return;
											
											DetermineDelta(hst.Select(i => new PackageAddress(i)), d.Package.Manifest, h, out incrementable, out List<Dependency> deps);
											
											//lock(Core.Filebase.Lock)

											//var file = caninc ? Aprvbase.Ipkg : Aprvbase.Cpkg;

											//deps	= incrementable ? deps : r.Manifest.CompleteDependencies.ToList();
											//Hash	= caninc ? Manifest.IncrementalHash :	Manifest.CompleteHash;
											//Length	= caninc ? Manifest.IncrementalLength :	Manifest.CompleteLength;
								
											foreach(var i in deps)
											{
												var dd = Download(i.Release, workflow);
																
												if(dd != null)
												{
													d.Dependencies.Add(dd);
												}
											}
										}

 										Core.Resources.GetFile(	package, 
																h, 
																incrementable ? Package.IncrementalFile : Package.CompleteFile, 
																incrementable ? d.Package.Manifest.IncrementalHash : d.Package.Manifest.CompleteHash, 
																workflow);


										Task.WaitAll(d.DependenciesRecursive.Select(i => i.Task).ToArray());

									done:
										lock(Lock)
										{
											d.Downloaded = true;
											Downloads.Remove(d);
										}
									},
									workflow.Cancellation.Token);
			
		
			return d;
		}

		//public void GetPackge(PackageAddress release, Workflow workflow)
		//{
		//	Task t = Task.CompletedTask;
		//
		//	lock(Lock)
		//	{
		//		if(!Exists(release))
		//		{
		//			var d = Download(release, workflow);
		//	
		//			t = d.Task;
		//		}
		//	}
		//
		//	t.Wait(workflow.Cancellation.Token);
		//}

	}
}
