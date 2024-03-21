using System.Reflection;

namespace Uccs.Uos
{
	public class Application
	{
		public NexusClient	Nexus;

		public Application()
		{
			Nexus = new NexusClient();

			AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
		}

		Assembly AssemblyResolve(object sender, ResolveEventArgs args)
		{
			var rp = Nexus.PackageHub.PathToAddress(args.RequestingAssembly.Location);

			var r = Nexus.PackageHub.Find(rp);

			foreach(var i in r.Manifest.CriticalDependencies)
			{
				var dp = Path.Join(Nexus.PackageHub.AddressToPath(i.Package), new AssemblyName(args.Name).Name + ".dll");

				if(File.Exists(dp))
				{
					return Assembly.LoadFile(dp);
				}
			}

			return null;
		}
	}
}
