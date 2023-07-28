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
		public const string		Ipkg = "ipkg";
		public const string		Cpkg = "cpkg";
		public const string		ManifestFile = "manifest";
		public const string		Removals = ".removals";
		public const string		DependenciesExt = "dependencies";
		public const string		Renamings = ".renamings"; /// TODO

		public ReleaseAddress	Address => new ReleaseAddress(Release.Address);
		ResourceBaseItem			Release;
		PackageBase				Packbase;
		Release					_Manifest;

		public Release	Manifest
		{
			get
			{
				if(_Manifest == null)
				{
					_Manifest = new Release(Packbase.Core.Zone){Address = Address};
					
					lock(Packbase.Filebase.Lock)
					{
						_Manifest.Read(new BinaryReader(new MemoryStream(Packbase.Filebase.ReadFile(Release.Address, ManifestFile))));
					}
				}

				return _Manifest;
			}
		}

		public Package(PackageBase packbase, ResourceBaseItem release)
		{
			Packbase = packbase;
			Release = release;
		}

		public Package(PackageBase packebase, ResourceBaseItem release, Release manifest)
		{
			Packbase = packebase;
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
		public ResourceBase				Filebase;
		string							ProductsPath;
		List<Package>					Packages = new();
		public Core						Core;
		List<PackageDownload>			Downloads = new();
		public object					Lock = new object();

		public PackageBase(Core core, ResourceBase filebase, string productspath)
		{
			Core = core;
			Filebase = filebase;
			ProductsPath = productspath;

			Directory.CreateDirectory(ProductsPath);
		}

		public string AddressToPath(ReleaseAddress release)
		{
			return Path.Join(ProductsPath, @$"{release.Author} {release.Product} {release.Realization}{Path.DirectorySeparatorChar}{release.Version}");
		}

		public ReleaseAddress PathToAddress(string path)
		{
			path = path.Substring(ProductsPath.Length);

			var i = path.IndexOf(Path.DirectorySeparatorChar);
			var apr = path.Substring(0, i).Split(' ');
			var v = path.Substring(i + 1);
			v = v.Substring(0, v.IndexOf(Path.DirectorySeparatorChar));

			return new ReleaseAddress(apr[0], apr[1], apr[2], Version.Parse(v));
		}

		public void Add(ReleaseAddress release, Release manifest)
		{
			var s = new MemoryStream();

			manifest.Write(new BinaryWriter(s));

			Add(release, s.ToArray());
		}

		public void Add(ReleaseAddress release, byte[] manifest)
		{
			lock(Filebase.Lock)
			{
				var r = Filebase.Add(release);

				Filebase.WriteFile(release, Package.ManifestFile, 0, manifest);

				Packages.Add(new Package(this, r));
			}
		}

		public Package Find(ReleaseAddress release)
		{
			var p = Packages.Find(i => i.Address == release);

			if(p != null)
				return p;

			ResourceBaseItem r;

			lock(Filebase.Lock)
				r = Filebase.Find(release);

			if(r != null)
			{
				p = new Package(this, r);
	
				Packages.Add(p);
	
				return p;
			}

			return null;
		}

		public bool ExistsRecursively(ReleaseAddress release)
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

		public string Add(ResourceAddress release, string distribution, IDictionary<string, string> files, IEnumerable<string> removals, Workflow workflow)
		{
			var zpath = Path.Join(Filebase.GetDirectory(release, true), distribution);

			using(var z = new FileStream(zpath, FileMode.Create))
			{
				using(var arch = new ZipArchive(z, ZipArchiveMode.Create))
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

			return zpath;
		}
		
		public string AddIncremental(ReleaseAddress release, Version previous, IDictionary<string, string> files, out Version minimal, Workflow workflow)
		{
			var rems = new List<string>();
			var incs = new Dictionary<string, string>();
			var olds = new List<string>();

			var r = Filebase.GetDirectory(release, true);

			r = r.Substring(0, r.LastIndexOf(' ') + 1);

			using(var s = new FileStream(Path.Join(r + previous, Package.Cpkg), FileMode.Open))
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
			
			return Add(release, Package.Ipkg, incs, rems, workflow);
		}

		public void DetermineDelta(IEnumerable<ReleaseAddress> history, Release manifest, out bool canincrement, out List<Dependency> dependencies)
		{
			dependencies = new();

			var dir = Filebase.GetDirectory(manifest.Address, false);

			if(Directory.Exists(dir))
			{
				var c = Directory.EnumerateFiles(dir, $"*.{Package.Cpkg}")	.Select(i => Version.Parse(Path.GetFileNameWithoutExtension(i)))
																			.OrderBy(i => i)
																			.TakeWhile(i => i < manifest.Address.Version) 
																			.FirstOrDefault();	/// find last complete package
				if(c != null) 
				{
					var need = history.SkipWhile(i => i.Version <= c).TakeWhile(i => i.Version < manifest.Address.Version);
					
					lock(Filebase.Lock)
						if(need.All(i => Filebase.Exists(i, Package.Ipkg)))
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
			}

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

		public void AddRelease(ReleaseAddress release, Version latestdeclared, IEnumerable<string> sources, string dependsdirectory, Workflow workflow)
		{
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

			var cpkg = Add(release, Package.Cpkg, files, new string[]{}, workflow);
			var ipkg = (string)null;

			if(latestdeclared != null)
			{
				ipkg = AddIncremental(release, latestdeclared, files, out minimal, workflow);
			}

			var vs = Directory.EnumerateFiles(dependsdirectory, $"*.{Package.DependenciesExt}").Select(i => Uccs.Version.Parse(Path.GetFileNameWithoutExtension(i))).Where(i => i < release.Version).OrderBy(i => i);

			IEnumerable<Dependency> acd = null;
			IEnumerable<Dependency> rcd = null;

			var f = Path.Join(dependsdirectory, $"{release.Version.ABC}.{Package.DependenciesExt}");
			
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

			var m = new Release(	Filebase.Zone,
									Filebase.Zone.Cryptography.HashFile(File.ReadAllBytes(cpkg)),
									new FileInfo(cpkg).Length,
									deps,
									ipkg != null ? Filebase.Zone.Cryptography.HashFile(File.ReadAllBytes(ipkg)) : null,
									ipkg != null ? new FileInfo(ipkg).Length : 0,
									minimal,
									acd,
									rcd);

			Add(release, m);
		}

		public void Unpack(ReleaseAddress release, bool overwrite = false)
		{
			var dir = Filebase.GetDirectory(release, false);

			var c = Directory.EnumerateFiles(dir, $"*.{Package.Cpkg}")	.Select(i => Version.Parse(Path.GetFileNameWithoutExtension(i)))
																		.OrderByDescending(i => i)
																		.SkipWhile(i => i > release.Version) /// skip younger
																		.FirstOrDefault();	/// find last available complete package
			if(c != null) 
			{
				var incs = Directory.EnumerateFiles(dir, $"*.{Package.Ipkg}")	.Select(i => Version.Parse(Path.GetFileNameWithoutExtension(i)))
																				.OrderBy(i => i)
																				.SkipWhile(i => i <= c)
																				.TakeWhile(i => i <= release.Version); /// take all incremetals before complete
				
				var deps = new List<Dependency>();

				void cunzip(Version v)
				{
					var r = new ReleaseAddress(release.Author, release.Product, release.Realization, v);

					using(var s = new FileStream(Filebase.AddressToPath(r, Package.Cpkg), FileMode.Open))
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
					var r = new ReleaseAddress(release.Author, release.Product, release.Realization, v);

					using(var s = new FileStream(Filebase.AddressToPath(r, Package.Ipkg), FileMode.Open))
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

		public ReleaseStatus GetStatus(ReleaseAddress package, int limit)
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
					var r = Filebase.Downloads.Find(i => i.Release == package);

					s.Download = new () {	File					= r.File,
											Length					= r.Length,
											CompletedLength			= r.CompletedLength,
											DependenciesRecursive	= p.DependenciesRecursive.Select(i => new DownloadReport.Dependency {Release = i.Address, Exists = Find(new ReleaseAddress(i.Address)) != null}).ToArray(),
											Hubs					= r.Hubs.Take(limit).Select(i => new DownloadReport.Hub { IP = i.Peer.IP, Seeds = i.Seeds.Take(limit).Select(i => i.IP), Status = i.Status }).ToArray(),
											Seeds					= r.Seeds.Take(limit).Select(i => new DownloadReport.Seed { IP = i.IP, Succeses = i.Succeses, Failures = i.Failures }).ToArray() };
				}
			}

			return s;
		}

		public void Pack(ReleaseAddress release, IEnumerable<string> sources, string dependsdirectory, Workflow workflow)
		{
			var qlatest = Core.Call(Role.Base, p => p.QueryResource($"{release.APR}/"), workflow);
			var previos = qlatest.Resources.OrderBy(i => ReleaseAddress.ParseVesion(i.Address.Resource)).FirstOrDefault();

			AddRelease(release, previos != null ? ReleaseAddress.ParseVesion(previos.Address.Resource) : null, sources, dependsdirectory, workflow);
		}

		public void Install(ReleaseAddress release, Workflow workflow)
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

		public PackageDownload Download(ReleaseAddress release, Workflow workflow)
		{
			if(Find(release) != null)
				return null;

			var d = Downloads.Find(j => j.Address == release);
				
			if(d != null)
				return d;
				
			d = new PackageDownload();
			Downloads.Add(d);

			d.Task = Task.Run(() =>	{
										IEnumerable<Resource> hst = null;

										while(!workflow.IsAborted)
										{
											try
											{
												hst = Core.Call(Role.Base, c => c.QueryResource(release.APR), workflow).Resources;
												break;
											}
											catch(RdcEntityException)
											{
												Thread.Sleep(100);
											}
										}
				
 										Core.Filebase.GetFile(release, Package.ManifestFile, workflow);

										lock(Lock)
										{
											d.Package = Find(release);
	 
	 										if(!d.Package.Manifest.GetOrCalcHash().SequenceEqual(hst.First(i => i.Address == release).Data))
	 											return;
																
											DetermineDelta(hst.Select(i => new ReleaseAddress(i.Address)), d.Package.Manifest, out bool incrementable, out List<Dependency> deps);
											
 											Core.Filebase.GetFile(release, incrementable ? Package.Ipkg : Package.Cpkg, workflow);

											//var file = caninc ? Aprvbase.Ipkg : Aprvbase.Cpkg;

											//deps	= incrementable ? deps : r.Manifest.CompleteDependencies.ToList();
											//Hash	= caninc ? Manifest.IncrementalHash :	Manifest.CompleteHash;
											//Length	= caninc ? Manifest.IncrementalLength :	Manifest.CompleteLength;
								
											foreach(var i in deps)
											{
												PackageDownload dd;

												dd = Download(i.Release, workflow);
																
												if(dd != null)
												{
													d.Dependencies.Add(dd);
												}
											}
										}

										while(workflow.Active)
										{
											Task.WaitAll(d.Tasks);

											lock(Lock)
												if(d.Succeeded)
													break;
										}

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
