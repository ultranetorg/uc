using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Uccs.Net
{
	public class ResourceBaseItem
	{
		public ResourceAddress	Address;
		public Availability		Availability;
		public List<Peer>		Hubs = new();

		public ResourceBaseItem(ResourceAddress address)
		{
			Address = address;
		}
	}
	
	public class ResourceBase
	{
		public const long				PieceMaxLength = 512 * 1024;
		public const string				StatusFile = ".status";

		string							PackagesPath;
		public List<ResourceBaseItem>	Releases = new();
		public List<Download>			Downloads = new();
		Thread							DeclaringThread;
		public Core						Core;
		public object					Lock = new object();
		public Zone						Zone;

		public ResourceBase(Core core, Zone zone, string packagespath)
		{
			Core = core;
			Zone = zone;

			PackagesPath = packagespath;

			Directory.CreateDirectory(PackagesPath);

			Releases =	Directory.EnumerateDirectories(PackagesPath)
						.SelectMany(r => 
						Directory.EnumerateFiles(r, "*")
						.Select(i => new ResourceBaseItem(PathToAddress(i)))).ToList();

			if(core != null && !core.IsClient)
			{
				DeclaringThread = new Thread(Declaring);
				DeclaringThread.Name = $"{Core.Settings.IP.GetAddressBytes()[3]} Declaring";
				DeclaringThread.Start();
			}
		}

		public ResourceAddress PathToAddress(string path)
		{
			path = path.Substring(PackagesPath.Length);

			var i = path.IndexOf(Path.DirectorySeparatorChar);

			return new ResourceAddress(path.Substring(0, i), path.Substring(i + 1).Replace(' ', '/'));
		}

		public static string ToRelative(ResourceAddress release)
		{
			return @$"{release.Author}{Path.DirectorySeparatorChar}{release.Resource.Replace('/', ' ')}";
		}

		public string AddressToPath(ResourceAddress release, string path)
		{
			return Path.Join(PackagesPath, ToRelative(release), path);
		}

		public string GetDirectory(ResourceAddress release, bool create)
		{
			var p = Path.Join(PackagesPath, ToRelative(release));

			if(create)
				Directory.CreateDirectory(p);
			return p;
		}

		public ResourceBaseItem Add(ResourceAddress release)
		{
			var r = new ResourceBaseItem(release);
	
			Releases.Add(r);
	
			return r;
		}

		public ResourceBaseItem Find(ResourceAddress release)
		{
			var r = Releases.Find(i => i.Address == release);

			if(r != null)
				return r;

			if(File.Exists(GetDirectory(release, false)))
			{
				r = new ResourceBaseItem(release);
	
				Releases.Add(r);
	
				return r;
			}

			return null;
		}

		public byte[] ReadFile(ResourceAddress release, string file, long offset, long length)
		{
			using(var s = new FileStream(AddressToPath(release, file), FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				s.Seek(offset, SeekOrigin.Begin);
				
				var b = new byte[Math.Min(length, PieceMaxLength)];
	
				s.Read(b);
	
				return b;
			}
		}

		public void WriteFile(ResourceAddress release, string file, long offset, byte[] data)
		{
			GetDirectory(release, true);

			using(var s = new FileStream(AddressToPath(release, file), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
			{
				s.Seek(offset, SeekOrigin.Begin);
				s.Write(data);
			}
		}

		public bool Exists(ResourceAddress release, string file)
		{
			return File.Exists(AddressToPath(release, file));
		}

		public byte[] ReadFile(ResourceAddress release, string file)
		{
			return File.ReadAllBytes(AddressToPath(release, file));
		}

		public byte[] Hashify(ResourceAddress release, string path)
		{
			return Zone.Cryptography.HashFile(File.ReadAllBytes(AddressToPath(release, path)));
		}

		public long GetLength(ResourceAddress release, string path)
		{
			return Exists(release, path) ? new FileInfo(AddressToPath(release, path)).Length : 0;
		}

		public Download DownloadFile(ResourceAddress release, string file, Workflow workflow)
		{
			if(Find(release) != null)
				return null;

			var d = Downloads.Find(j => j.Release == release);
				
			if(d == null)
			{
				d = new Download(Core, release, file, workflow);
				Downloads.Add(d);
			}
		
			return d;
		}

		public void GetFile(ResourceAddress release, string file, Workflow workflow)
		{
			Task t = Task.CompletedTask;

			lock(Lock)
			{
				if(!Exists(release, file))
				{
					var d = DownloadFile(release, file, workflow);
			
					t = d.Task;
				}
			}

			t.Wait(workflow.Cancellation.Token);
		}

		void Declaring()
		{
			Core.Workflow.Log?.Report(this, "Declaring started");

			try
			{
				while(Core.Workflow.IsAborted)
				{
					Core.Workflow.Wait(100);

					ResourceBaseItem[] rs;
					List<Peer> used;

					lock(Lock)
					{
						rs = Releases.Where(i => i.Hubs.Count < 8).ToArray();
						used = rs.SelectMany(i => i.Hubs).Distinct().Where(h => rs.All(r => r.Hubs.Contains(h))).ToList();
					}

					if(rs.Any())
					{
						Core.Call(	Role.Hub, 
									h => {
											lock(Lock)
											{
												rs = rs.Where(i => !i.Hubs.Contains(h)).ToArray();
												used.Add(h);
											}
																				
											h.DeclareRelease(rs.ToDictionary(i => i.Address, i => i.Availability));

											lock(Lock)
											{
												foreach(var i in rs)
													i.Hubs.Add(h);
											}
										},
									Core.Workflow,
									used,
									true);
					}
				}
			}
			catch(OperationCanceledException)
			{
			}
			catch(Exception ex) when (!Debugger.IsAttached)
			{
				Core.Stop(MethodBase.GetCurrentMethod(), ex);
			}
		}
	}
}
