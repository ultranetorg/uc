using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Uccs.Net;
using Uccs.Nexus;
using Uccs.Rdn;
using Uccs.Vault;

namespace Uccs.Nexus.Windows;

internal class Program: ApplicationContext
{
	[STAThread]
	static void Main()
	{
		ApplicationConfiguration.Initialize();
		System.Windows.Forms.Application.Run(new NexusSystem());
	}

	public class NexusSystem : ApplicationContext
	{
		public static string		ExeDirectory;
		Nexus						Nexus;

		public NexusSystem()
		{
			ExeDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
		
			var b = new NetBoot(ExeDirectory);
			var ns = new NexusSettings(b.Zone, b.Profile) {Name = Guid.NewGuid().ToString()};
			var vs = new VaultSettings(b.Profile, b.Zone);
		
			Nexus = new Nexus(b, ns, vs, new RealClock(), new Flow(nameof(Nexus), new Log()));
			Nexus.RunRdn(null);
		}
	}
}
