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

public class Program: ApplicationContext
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

		public static void InitializeAuthUI(Nexus nexus)
		{
			nexus.Vault.AuthenticationRequested =	(appplication, net, suggested) =>
													{
														var f = new AuthenticattionForm(nexus.Vault);

														f.Account = suggested;
														f.SetApplication(appplication);
														f.SetNet(net);
													
														if(f.ShowDialog() == DialogResult.OK)
														{
															return new AuthenticationChoice {Account = f.Account, Trust = f.Trust};
														}
														else
														{
															return null;
														}
													};

			nexus.Vault.AuthorizationRequested  =	(appplication, net, signer, operation) =>
													{
														var f = new AuthorizationForm();

														f.SetApplication(appplication);
														f.SetNet(net);
														f.SetSigner(signer);
														f.SetOperation(operation);
													
														if(f.ShowDialog() == DialogResult.OK)
														{
															return true;
														}
														else
														{
															return false;
														}
													};
		}
	}
}
