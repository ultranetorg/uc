using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Nethereum.RLP;

namespace UC.Net
{
	public class Release
	{
		public VersionAddress	Address;
		public List<Peer>		Hubs = new();
		Filebase				Filebase;
		Manifest				_Manifest;

		public Manifest	Manifest
		{
			get
			{
				if(_Manifest == null)
				{
					_Manifest = new Manifest{Release = Address};
	
					using(var s = File.OpenRead(Path.Join(Filebase.GetDirectory(Address), Address.Version + $".{Filebase.ManifestExt}")))
					{
						_Manifest.Read(new BinaryReader(s));
					}
				}

				return _Manifest;
			}
		}

		public Release(Filebase filebase, VersionAddress address)
		{
			Filebase = filebase;
			Address = address;
		}

		public Release(Filebase filebase, VersionAddress address, Manifest manifest)
		{
			Filebase = filebase;
			Address = address;
			_Manifest = manifest;
		}

		public override string ToString()
		{
			return Address.ToString();
		}
	}

	public class Filebase
	{
		public const string		Ipkg = "ipkg";
		public const string		Cpkg = "cpkg";
		public const string		ManifestExt = "manifest";
		public const string		Removals = ".removals";
		public const string		DependenciesExt = "dependencies";
		public const string		Renamings = ".renamings"; /// TODO
		public const long		PieceMaxLength = 64 * 1024;

		string					Root;
		public List<Release>	Releases = new();

		public Filebase(string path)
		{
			Root = path;

			Directory.CreateDirectory(Root);

			Releases =	Directory.EnumerateDirectories(Root).SelectMany(a => 
						Directory.EnumerateDirectories(a).SelectMany(p => 
						Directory.EnumerateDirectories(p).SelectMany(r => 
						Directory.EnumerateFiles(r, $"*.{ManifestExt}").Select(i =>	{
																						return new Release(this, new (	Path.GetFileName(a), 
																														Path.GetFileName(p), 
																														Path.GetFileName(r), 
																														Version.Parse(Path.GetFileNameWithoutExtension(i))));
																					})))).ToList();
		}
				
		string ToPath(VersionAddress package, Distributive distributive)
		{
			return Path.Join(Root, package.Author, package.Product, package.Realization, $"{package.Version}.{(distributive == Distributive.Complete ? Cpkg : Ipkg)}");
		}

		internal string GetDirectory(VersionAddress release)
		{
			var p = Path.Join(Root, release.Author, release.Product, release.Realization);
			Directory.CreateDirectory(p);
			return p;
		}

		public void AddRelease(VersionAddress release, Manifest manifest)
		{
			var p = Path.Join(GetDirectory(release), release.Version + $".{ManifestExt}");

			using(var s = File.OpenWrite(p))
			{
				manifest.Write(new BinaryWriter(s));
			}

			Releases.Add(new Release(this, release, manifest));
		}

		public void AddRelease(VersionAddress release, byte[] manifest)
		{
			var p = Path.Join(GetDirectory(release), release.Version + $".{ManifestExt}");

			File.WriteAllBytes(p, manifest);

			Releases.Add(new Release(this, release));
		}

		public Release FindRelease(VersionAddress release)
		{
			var r = Releases.Find(i => i.Address == release);

			if(r != null)
				return r;

			var p = Path.Join(GetDirectory(release), release.Version + $".{ManifestExt}");

			if(File.Exists(p))
			{
				r = new Release(this, release);
	
				Releases.Add(r);
	
				return r;
			}

			return null;
		}

		public byte[] ReadManifest(VersionAddress release)
		{
			var p = Path.Join(GetDirectory(release), release.Version + $".{ManifestExt}");

			return File.ReadAllBytes(p);
		}

		public string Add(VersionAddress release, Distributive distribution, IDictionary<string, string> files, IEnumerable<string> removals, Workflow workflow)
		{
			var zpath = Path.Join(GetDirectory(release), $"{release.Version}.{(distribution == Distributive.Complete ? Cpkg : Ipkg)}");

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
						var e = arch.CreateEntry(Removals);
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
		
		public string AddIncremental(VersionAddress release, Version previous, IDictionary<string, string> files, out Version minimal, Workflow workflow)
		{
			var rems = new List<string>();
			var incs = new Dictionary<string, string>();
			var olds = new List<string>();

			var rlz = GetDirectory(release);

			using(var s = new FileStream(Path.Join(rlz, $"{previous}.{Cpkg}"), FileMode.Open))
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
			
			return Add(release, Distributive.Incremental, incs, rems, workflow);
		}

		public bool Exists(VersionAddress release, Distributive distributive)
		{
			return File.Exists(ToPath(release, distributive));
		}

		public bool ExistsRecursively(VersionAddress release)
		{
			var r = FindRelease(release);

			if(r == null)
				return false;

			foreach(var i in r.Manifest.CompleteDependencies)
			{
				if(i.Type == DependencyType.Critical && !ExistsRecursively(i.Release))
					return false;
			}

			return true;
		}

		public void DetermineDelta(IEnumerable<ReleaseRegistration> history, Manifest manifest, out Distributive distributive, out List<Dependency> dependencies)
		{
			dependencies = new();

			var dir = GetDirectory(manifest.Release);

			if(Directory.Exists(dir))
			{
				var c = Directory.EnumerateFiles(dir, $"*.{Cpkg}")	.Select(i => Version.Parse(Path.GetFileNameWithoutExtension(i)))
																	.OrderBy(i => i)
																	.TakeWhile(i => i < manifest.Release.Version) 
																	.FirstOrDefault();	/// find last complete package
				if(c != null) 
				{
					var need = history.SkipWhile(i => i.Release.Version <= c).TakeWhile(i => i.Release.Version < manifest.Release.Version);
					
					if(need.All(i => Exists(i.Release, Distributive.Incremental)))
					{
						foreach(var i in need)
						{
							var r = FindRelease(i.Release);

							dependencies.AddRange(r.Manifest.AddedDependencies);
							dependencies.RemoveAll(j => r.Manifest.RemovedDependencies.Contains(j));
						}
						
						dependencies.AddRange(manifest.AddedDependencies);
						dependencies.RemoveAll(j => manifest.RemovedDependencies.Contains(j));
						distributive = Distributive.Incremental; /// we have all incremental packages since last complete one

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

			distributive = Distributive.Complete;
		}

		public byte[] ReadPackage(VersionAddress release, Distributive distributive, long offset, long length)
		{
			using(var s = new FileStream(ToPath(release, distributive), FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				s.Seek(offset, SeekOrigin.Begin);
				
				var b = new byte[Math.Min(length, PieceMaxLength)];
	
				s.Read(b);
	
				return b;
			}
		}

		public byte[] ReadPackage(VersionAddress release, Distributive distributive)
		{
			return File.ReadAllBytes(ToPath(release, distributive));
		}

		public byte[] GetHash(VersionAddress release, Distributive distributive)
		{
			return Cryptography.Current.Hash(File.ReadAllBytes(ToPath(release, distributive)));
		}

		public long GetLength(VersionAddress release, Distributive distributive)
		{
			return Exists(release, distributive) ? new FileInfo(ToPath(release, distributive)).Length : 0;
		}

		public void WritePackage(VersionAddress release, Distributive distributive, long offset, byte[] data)
		{
			GetDirectory(release);

			using(var s = new FileStream(ToPath(release, distributive), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
			{
				s.Seek(offset, SeekOrigin.Begin);
				s.Write(data);
			}
		}

		public void AddRelease(VersionAddress release, Version latestdeclared, IEnumerable<string> sources, string dependsdirectory, Workflow workflow)
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

			var cpkg = Add(release, Distributive.Complete, files, new string[]{}, workflow);
			var ipkg = (string)null;

			if(latestdeclared != null)
			{
				ipkg = AddIncremental(release, latestdeclared, files, out minimal, workflow);
			}

			var vs = Directory.EnumerateFiles(dependsdirectory, $"*.{DependenciesExt}").Select(i => UC.Version.Parse(Path.GetFileNameWithoutExtension(i))).Where(i => i < release.Version).OrderBy(i => i);

			IEnumerable<Dependency> acd = null;
			IEnumerable<Dependency> rcd = null;

			var f = Path.Join(dependsdirectory, $"{release.Version.ABCD}.{DependenciesExt}");
			
			//var deps = File.Exists(f) ? File.ReadLines(f).Select(i => ReleaseAddress.Parse(i)) : new ReleaseAddress[]{};
			var deps = File.Exists(f) ? new XonDocument(File.ReadAllText(f)).Nodes.Select(i => Dependency.From(i))
									  : new Dependency[]{};

			
			if(vs.Any())
			{
				//var lastdeps = File.ReadLines(Path.Join(dependsdirectory, $"{vs.Last()}.{DependenciesExt}")).Select(i => ReleaseAddress.Parse(i));
				var lastdeps = new XonDocument(File.ReadAllText(Path.Join(dependsdirectory, $"{vs.Last()}.{DependenciesExt}"))).Nodes.Select(i => Dependency.From(i));
		
				acd = deps.Where(i => !lastdeps.Contains(i));
				rcd = lastdeps.Where(i => !deps.Contains(i));
			}
			else
				acd = deps;

			var m = new Manifest(	Cryptography.Current.Hash(File.ReadAllBytes(cpkg)),
									new FileInfo(cpkg).Length,
									deps,
									ipkg != null ? Cryptography.Current.Hash(File.ReadAllBytes(ipkg)) : null,
									ipkg != null ? new FileInfo(ipkg).Length : 0,
									minimal,
									acd,
									rcd);

			AddRelease(release, m);


			//if(ipkg != null)
			//	DeclarePackage(new[]{new PackageAddress(release, Distributive.Complete), new PackageAddress(release, Distributive.Incremental)}, workflow);
			//else
			//	DeclarePackage(new[]{new PackageAddress(release, Distributive.Complete)}, workflow);

			//return o;
		}

		public void Unpack(VersionAddress release, string productsroot, bool overwrite = false)
		{
			var dir = GetDirectory(release);

			var c = Directory.EnumerateFiles(dir, $"*.{Cpkg}")	.Select(i => Version.Parse(Path.GetFileNameWithoutExtension(i)))
																.OrderByDescending(i => i)
																.SkipWhile(i => i > release.Version) /// skip younger
																.FirstOrDefault();	/// find last available complete package
			if(c != null) 
			{
				var incs = Directory.EnumerateFiles(dir, $"*.{Ipkg}")	.Select(i => Version.Parse(Path.GetFileNameWithoutExtension(i)))
																		.OrderBy(i => i)
																		.SkipWhile(i => i <= c)
																		.TakeWhile(i => i <= release.Version); /// take all incremetals before complete
				
				var aprr = @$"{release.Author}-{release.Product}-{release.Realization}{Path.DirectorySeparatorChar}{release.Version}";

				var deps = new List<Dependency>();

				void cunzip(Version v)
				{
					var r = new VersionAddress(release.Author, release.Product, release.Realization, v);

					using(var s = new FileStream(ToPath(r, Distributive.Complete), FileMode.Open))
					{
						using(var arch = new ZipArchive(s, ZipArchiveMode.Read))
						{
							foreach(var e in arch.Entries)
							{
								var f = Path.Join(productsroot, aprr, e.FullName.Replace('/', Path.DirectorySeparatorChar));
								
								if(!File.Exists(f) || overwrite)
								{
									Directory.CreateDirectory(Path.GetDirectoryName(f));
									e.ExtractToFile(f, true);
								}
							}
						}
					}

					var m = FindRelease(r);

					foreach(var i in m.Manifest.CompleteDependencies)
					{
						Unpack(i.Release, productsroot);
					}

					deps.AddRange(m.Manifest.CompleteDependencies);
				}

				cunzip(c);

				void iunzip(Version v)
				{
					var r = new VersionAddress(release.Author, release.Product, release.Realization, v);

					using(var s = new FileStream(ToPath(r, Distributive.Incremental), FileMode.Open))
					{
						using(var arch = new ZipArchive(s, ZipArchiveMode.Read))
						{
							foreach(var e in arch.Entries)
							{
								if(e.Name != Removals)
								{
									var f = Path.Join(productsroot, aprr, e.FullName.Replace('/', Path.DirectorySeparatorChar));
									
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
											File.Delete(Path.Join(productsroot, sr.ReadLine()));
										}
									}
								}
							}
						}
					}

					var m = FindRelease(r);

					foreach(var i in m.Manifest.AddedDependencies)
					{
						Unpack(i.Release, productsroot);
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
					var f = Path.Join(productsroot, $"{aprr}.{DependenciesExt}");
					
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
	}
}
