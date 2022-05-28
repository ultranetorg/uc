using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UC.Net
{
	public class Filebase
	{
		string Root;

		const string IPKG = "ipkg";
		const string CPKG = "cpkg";
		const string REMOVALS = ".removals";
		const string RENAMINGS = ".renamings"; /// TODO
		public const long PieceLenghtMax = 1024 * 1024;

		public Filebase(Settings settings)
		{
			Root = System.IO.Path.Join(settings.Profile, "Filebase");

			Directory.CreateDirectory(Root);
		}
		
		string ToPath(ReleaseAddress address, Distribution distibution)
		{
			return Path.Join(Root, address.Author, address.Product, $"{address.Version}__{address.Platform}.{(distibution == Distribution.Complete ? CPKG : IPKG)}");
		}

		public string Add(ReleaseAddress release, IDictionary<string, string> files, Distribution distribution, List<string> removals = null)
		{
			var ap = Path.Join(Root, release.Author, release.Product);

			Directory.CreateDirectory(ap);
			
			var zpath = Path.Join(ap, $"{release.Version}__{release.Platform}.{(distribution == Distribution.Complete ? CPKG : IPKG)}");

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
						var e = arch.CreateEntry(REMOVALS);
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

			var history = Directory.EnumerateFiles(prod, $"*.*.*.*__{release.Platform}.{CPKG}").Select(i => Version.Parse(Path.GetFileNameWithoutExtension(i).Split("__")[0])).Where(i => i != release.Version);

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

			using(var s = new FileStream(Path.Join(prod, $"{previous}__{release.Platform}.{CPKG}"), FileMode.Open))
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
			
			return Add(release, incs, Distribution.Incremental, rems);
		}

		public byte[] ReadPackage(DownloadPackageRequest request)
		{
			using(var s = new FileStream(ToPath(request, request.Distribution), FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				s.Seek(request.Offset, SeekOrigin.Begin);
				
				var b = new byte[Math.Min(request.Length, PieceLenghtMax)];
	
				s.Read(b);
	
				return b;
			}
		}

		public long GetLength(ReleaseAddress release, Distribution distribution)
		{
			return new FileInfo(ToPath(release, distribution)).Length;
		}

		public void Append(ReleaseAddress release, Distribution distribution, byte[] data)
		{
			using(var s = new FileStream(ToPath(release, distribution), FileMode.Append, FileAccess.Write, FileShare.Read))
			{
				s.Write(data);
			}
		}
	}
}
