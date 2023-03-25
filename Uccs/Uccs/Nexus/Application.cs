using System;
using System.IO;
using System.Reflection;
using Uccs.Net;

namespace Uccs.Uos
{
	public class Application
	{
		public string	ProductsPath;
		public Client	Sun;

		public Application()
		{
			ProductsPath = Environment.GetEnvironmentVariable(Nexus.BootProductsPath);

			Sun = new Client(	Environment.GetEnvironmentVariable(Nexus.BootSunAddress), 
								Environment.GetEnvironmentVariable(Nexus.BootSunApiKey), 
								Zone.ByName(Environment.GetEnvironmentVariable(Nexus.BootZone)), 
								ProductsPath);

			AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
		}

		Assembly AssemblyResolve(object sender, ResolveEventArgs args)
		{
			var rp = ReleaseFromAssembly(args.RequestingAssembly.Location);

			var r = Sun.Filebase.FindRelease(rp);

			foreach(var i in r.Manifest.CriticalDependencies)
			{
				var dp = Path.Join(MapReleasePath(i.Release), new AssemblyName(args.Name).Name + ".dll");

				if(File.Exists(dp))
				{
					return Assembly.LoadFile(dp);
				}
			}

			return null;
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
			return Path.Join(ProductsPath, $"{release.Author}-{release.Product}-{release.Realization}", release.Version.ABC);
		}

		public void GetRelease(ReleaseAddress version, Workflow workflow)
		{
			Sun.GetRelease(version, workflow);
		}
	}
}
