using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Uccs.Net;

namespace Uccs.Uos
{
	public class NexusClient
	{
		public string ProductsPath;	
		public string SunAddress; 		
		public string SunApiKey;
		public Zone Zone;

		public Filebase		Filebase;
		public JsonClient	Sun;
		HttpClient			Http = new HttpClient();

		public NexusClient()
		{
			ProductsPath	=	Environment.GetEnvironmentVariable(Nexus.BootProductsPath);
			SunAddress		= 	Environment.GetEnvironmentVariable(Nexus.BootSunAddress);
			SunApiKey		=	Environment.GetEnvironmentVariable(Nexus.BootSunApiKey);
			Zone			=	null;//Environment.GetEnvironmentVariable(Nexus.BootZone);

			Sun = new JsonClient(Http, SunAddress, Zone, SunApiKey);

			var s = Sun.GetSettings(new Workflow());

			Filebase = new Filebase(Zone, Path.Join(s.ProfilePath, nameof(Filebase)), ProductsPath);
		}

		string MapPath(ReleaseAddress r)
		{
			return Path.Join(ProductsPath, $"{r.Author}-{r.Product}-{r.Realization}", r.Version.ABC);
		}

		public ReleaseAddress ReleaseFromAssembly(string path)
		{
			var x = path.Substring(ProductsPath.Length).Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

			var apr = x[0].Split('-');

			return new ReleaseAddress(apr[0], apr[1], apr[2], Version.Parse(x[1]));
		}

		public static string VersionToRelative(ReleaseAddress release)
		{
			return Path.Join($"{release.Author}-{release.Product}-{release.Realization}", release.Version.ABC);
		}

		public string MapReleasePath(ReleaseAddress release)
		{
			return Path.Join(ProductsPath, VersionToRelative(release));
		}

		public void Start(Uri address, Workflow workflow)
		{
			var s = Sun.GetReleaseStatus(ReleaseAddress.Parse(address.LocalPath), workflow);

			if(s.Manifest == null)
			{
				Sun.GetRelease(ReleaseAddress.Parse(address.LocalPath), workflow);

				while(true)
				{
					workflow.Wait(1);

					s = Sun.GetReleaseStatus(ReleaseAddress.Parse(address.LocalPath), workflow);

					if(s.Download == null)
					{
						Execute(address);
						return;
					}
				}
			}
		}

		void Execute(Uri request)
		{
			var r = ReleaseAddress.Parse(request.LocalPath);

			var f = Directory.EnumerateFiles(MapPath(r), "*.start").FirstOrDefault();
			
			if(f != null)
			{
				string setenv(ReleaseAddress a, string p)
				{
					p += ";" + MapPath(a);

					foreach(var i in Filebase.FindRelease(a).Manifest.CompleteDependencies.Where(i => i.Type == DependencyType.Critical && i.Flags.HasFlag(DependencyFlag.SideBySide)))
					{
						p += ";" + setenv(i.Release, p);
					}

					return p;
				}

				Environment.SetEnvironmentVariable("PATH", setenv(r, Environment.GetEnvironmentVariable("PATH")));
				
				var s = new XonDocument(File.ReadAllText(f));
				
				Process.Start(s.GetString("Executable"), s.GetString("Arguments"));
			}
		}
	}
}
