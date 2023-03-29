using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using Uccs.Net;

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
			var rp = Nexus.ReleaseFromAssembly(args.RequestingAssembly.Location);

			var r = Nexus.Filebase.FindRelease(rp);

			foreach(var i in r.Manifest.CriticalDependencies)
			{
				var dp = Path.Join(Nexus.MapReleasePath(i.Release), new AssemblyName(args.Name).Name + ".dll");

				if(File.Exists(dp))
				{
					return Assembly.LoadFile(dp);
				}
			}

			return null;
		}
	}
}
