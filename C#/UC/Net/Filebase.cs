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
		//Core										Core;
		string										Root;

		const string								Ipkg = "ipkg";
		const string								Cpkg = "cpkg";
		const string								Removals = ".removals";
		const string								Renamings = ".renamings"; /// TODO
		public const long							PieceMaxLength = 64 * 1024;

		public Filebase(Settings settings)
		{
			Root = System.IO.Path.Join(settings.Profile, "Filebase");

			Directory.CreateDirectory(Root);
		}
				
		string ToPath(PackageAddress package)
		{
			return Path.Join(Root, package.Author, package.Product, $"{package.Version}__{package.Platform}.{(package.Distribution == Distribution.Complete ? Cpkg : Ipkg)}");
		}

		public void WriteInstalled(BinaryWriter writer)
		{
			foreach(var a in Directory.EnumerateDirectories(Root))
			{
				foreach(var p in Directory.EnumerateDirectories(a))
				{
					var packs = Directory.EnumerateFiles(p, $"*").GroupBy(i => Version.Parse(Path.GetFileNameWithoutExtension(i).Split("__")[0]));

					writer.WriteUtf8(a);
					writer.WriteUtf8(p);
					writer.Write(packs.Min(i => i.Key));
					writer.Write(packs.Max(i => i.Key));
				}
			}
		}

		public string Add(ReleaseAddress release, Distribution distribution, IDictionary<string, string> files, List<string> removals = null)
		{
			var ap = Path.Join(Root, release.Author, release.Product);

			Directory.CreateDirectory(ap);
			
			var zpath = Path.Join(ap, $"{release.Version}__{release.Platform}.{(distribution == Distribution.Complete ? Cpkg : Ipkg)}");

			using(var z = new FileStream(zpath, FileMode.Create))
			{
				using(var arch = new ZipArchive(z, ZipArchiveMode.Create))
				{
					foreach(var f in files)
					{
						arch.CreateEntryFromFile(f.Key, f.Value);
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
		
		public string AddIncremental(ReleaseAddress release, IDictionary<string, string> files, out Version previous, out Version minimal)
		{
			var rems = new List<string>();
			var incs = new Dictionary<string, string>();
			var olds = new List<string>();

			var prod = Path.Join(Root, release.Author, release.Product);

			var history = Directory.EnumerateFiles(prod, $"*.*.*.*__{release.Platform}.{Cpkg}").Select(i => Version.Parse(Path.GetFileNameWithoutExtension(i).Split("__")[0])).Where(i => i != release.Version);

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

			using(var s = new FileStream(Path.Join(prod, $"{previous}__{release.Platform}.{Cpkg}"), FileMode.Open))
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
							}
						}
						else /// a file is removed in the new package
						{
							rems.Add(e.FullName);
						}
					}
				}
			}

			foreach(var f in files)
			{
				if(!olds.Contains(f.Value)) /// a completely new file
				{
					incs.Add(f.Key, f.Value);
				}
			}

			minimal = previous; /// TODO: determine really minimal
			
			return Add(release, Distribution.Incremental, incs, rems);
		}

		public bool Exists(PackageAddress package)
		{
			return File.Exists(ToPath(package));
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
			var dir = Path.Join(Root, package.Author, package.Product);

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
