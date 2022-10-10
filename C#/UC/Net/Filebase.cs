using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UC.Net
{
	public class Filebase
	{
		string				Root;

		const string		Ipkg = "ipkg";
		const string		Cpkg = "cpkg";
		const string		ManifestExt = "manifest";
		const string		Removals = ".removals";
		const string		Renamings = ".renamings"; /// TODO
		public const long	PieceMaxLength = 64 * 1024;

		List<Manifest>		Manifests = new();

		public Filebase(Settings settings)
		{
			Root = System.IO.Path.Join(settings.Profile, "Filebase");

			Directory.CreateDirectory(Root);
		}
				
		string ToPath(PackageAddress package)
		{
			return Path.Join(Root, package.Author, package.Product, package.Platform, $"{package.Version}.{(package.Distributive == Distributive.Complete ? Cpkg : Ipkg)}");
		}

		public PackageAddress[] GetAll()
		{
			return Directory.EnumerateDirectories(Root).SelectMany(a => 
					Directory.EnumerateDirectories(a).SelectMany(p => 
						Directory.EnumerateDirectories(p).SelectMany(r => 
							Directory.EnumerateFiles(r, $"*").Select(i =>	{
																				return new PackageAddress(	Path.GetFileName(a), 
																											Path.GetFileName(p), 
																											Path.GetFileName(r), 
																											Version.Parse(Path.GetFileNameWithoutExtension(i)),
																											Path.GetExtension(i)[1] == 'c' ? Distributive.Complete : Distributive.Incremental);
																			})))).ToArray();
		}

		public void AddManifest(ReleaseAddress release, Manifest manifest)
		{
			var p = Path.Join(Root, release.Author, release.Product, release.Platform, release.Version + $".{ManifestExt}");

			using(var s = File.OpenWrite(p))
			{
				manifest.Write(new BinaryWriter(s));
			}
			//manifest.ToXon(new XonTextValueSerializator()).Save(new XonTextWriter(File.OpenWrite(p), Encoding.UTF8));
		}

		public Manifest GetManifest(ReleaseAddress release)
		{
			var r = Manifests.Find(i => i.Release == release);

			if(r != null)
				return r;

			var m = new Manifest(){Release = release};

			var p = Path.Join(Root, release.Author, release.Product, release.Platform, release.Version + $".{ManifestExt}");

			using(var s = File.OpenRead(p))
			{
				m.Read(new BinaryReader(s));
			}

			Manifests.Add(m);

			return m;
		}

		public string Add(ReleaseAddress release, Distributive distribution, IDictionary<string, string> files, List<string> removals, Workflow workflow)
		{
			var ap = Path.Join(Root, release.Author, release.Product, release.Platform);

			Directory.CreateDirectory(ap);
			
			var zpath = Path.Join(ap, $"{release.Version}.{(distribution == Distributive.Complete ? Cpkg : Ipkg)}");

			using(var z = new FileStream(zpath, FileMode.Create))
			{
				using(var arch = new ZipArchive(z, ZipArchiveMode.Create))
				{
					foreach(var f in files)
					{
						arch.CreateEntryFromFile(f.Key, f.Value);
						workflow?.Log?.Report(this, "Packed", f.Value);
					}

					if(removals != null)
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
		
		public string AddIncremental(ReleaseAddress release, IDictionary<string, string> files, out Version previous, out Version minimal, Workflow workflow)
		{
			var rems = new List<string>();
			var incs = new Dictionary<string, string>();
			var olds = new List<string>();

			var rlz = Path.Join(Root, release.Author, release.Product, release.Platform);

			var history = Directory.EnumerateFiles(rlz, $"*.*.*.*.{Cpkg}").Select(i => Version.Parse(Path.GetFileNameWithoutExtension(i))).Where(i => i != release.Version);

			if(!history.Any())
			{
				previous = Version.Zero;
				minimal = Version.Zero;
				return null;
			}
			else
			{
				previous = history.Max();
			}

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

								var abuffer = new byte[16*1024];
								var bbuffer = new byte[16*1024];

								while(true)
								{
									var n = a.Read(abuffer, 0, abuffer.Length);
									var m = b.Read(bbuffer, 0, bbuffer.Length);

									if(n == 0 && m == 0)
										break;

									if(n != m)
									{
										changed = true;
										break;
									}
								
									if(!abuffer.SequenceEqual(bbuffer))
									{
										changed = true;
										break;
									}
								}

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

		public bool Exists(PackageAddress package)
		{
			return File.Exists(ToPath(package));
		}

		public void DetermineDelta(IEnumerable<ReleaseRegistration> history, ReleaseAddress release, out Distributive distributive, out List<ReleaseAddress> dependencies)
		{
			dependencies = new();

			var dir = Path.Join(Root, release.Author, release.Product, release.Platform);

			if(Directory.Exists(dir))
			{
				var c = Directory.EnumerateFiles(dir, $"*.{Cpkg}")	.Select(i => Version.Parse(Path.GetFileNameWithoutExtension(i)))
																	.OrderBy(i => i)
																	.TakeWhile(i => i < release.Version) 
																	.FirstOrDefault();	/// find last complete package
				if(c != null) 
				{
					var need = history.SkipWhile(i => i.Release.Version <= c).TakeWhile(i => i.Release.Version < release.Version);
					
					if(need.All(i => Exists(new PackageAddress(i.Release, Distributive.Incremental))))
					{
						foreach(var i in need)
						{
							var m = GetManifest(i.Release);

							dependencies.AddRange(m.AddedDependencies);
							dependencies.RemoveAll(j => m.RemovedDependencies.Contains(j));
						}
						
						var t = GetManifest(release);

						dependencies.AddRange(t.AddedDependencies);
						dependencies.RemoveAll(j => t.RemovedDependencies.Contains(j));
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

		public byte[] ReadPackage(PackageAddress package, long offset, long length)
		{
			using(var s = new FileStream(ToPath(package), FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				s.Seek(offset, SeekOrigin.Begin);
				
				var b = new byte[Math.Min(length, PieceMaxLength)];
	
				s.Read(b);
	
				return b;
			}
		}

		public byte[] GetHash(PackageAddress package)
		{
			return Cryptography.Current.Hash(File.ReadAllBytes(ToPath(package)));
		}

		public long GetLength(PackageAddress package)
		{
			return File.Exists(ToPath(package)) ? new FileInfo(ToPath(package)).Length : 0;
		}

		public void Write(PackageAddress package, long offset, byte[] data)
		{
			var dir = Path.GetDirectoryName(ToPath(package));

			if(!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			using(var s = new FileStream(ToPath(package), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
			{
				s.Seek(offset, SeekOrigin.Begin);
				s.Write(data);
			}
		}

	}
}
