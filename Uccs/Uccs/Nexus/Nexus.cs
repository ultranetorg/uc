using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UC.Net;

namespace UC
{
	public class Nexus
	{
		public string	ProductsPath;
		public Client	Sun;

		public Nexus(string productspath, Zone zone)
		{
			ProductsPath = productspath;
			Sun = new Client("192.168.1.107", null, zone, ProductsPath);
			AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
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

		public void GetRelease(ReleaseAddress version, Workflow workflow)
		{
			Sun.GetRelease(version, workflow);
		}
	}
}
