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

		public ResourceHub		Filebase;
		public PackageHub	PackageBase;
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

			Filebase = new ResourceHub(null, Zone, Path.Join(s.ProfilePath, nameof(Filebase)));
			PackageBase = new PackageHub(null, Filebase, ProductsPath);
		}

		public void Start(Uri address, Workflow workflow)
		{
			var s = Sun.GetReleaseStatus(PackageAddress.Parse(address.LocalPath), workflow);

			if(s.Manifest == null)
			{
				Sun.InstallPackage(PackageAddress.Parse(address.LocalPath), workflow);

				while(true)
				{
					workflow.Wait(1);

					s = Sun.GetReleaseStatus(PackageAddress.Parse(address.LocalPath), workflow);

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
			var r = PackageAddress.Parse(request.LocalPath);

			var f = Directory.EnumerateFiles(PackageBase.AddressToPath(r), "*.start").FirstOrDefault();
			
			if(f != null)
			{
				string setenv(PackageAddress a, string p)
				{
					p += ";" + PackageBase.AddressToPath(a);

					foreach(var i in PackageBase.Find(a).Manifest.CompleteDependencies.Where(i => i.Type == DependencyType.Critical && i.Flags.HasFlag(DependencyFlag.SideBySide)))
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
