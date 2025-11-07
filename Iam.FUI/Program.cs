using System.Reflection;
using Uccs.Net;
using Uccs.Nexus;

namespace Uccs.Iam.FUI;

public class Iam
{
	public static string		ExeDirectory;
	public NexusApiClient		Nexus;
	public VaultApiClient		Vault;

	[STAThread]
	static void Main()
	{
		ApplicationConfiguration.Initialize();
		//Application.SetColorMode(SystemColorMode.Dark);
		
		ExeDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
				
		var b = new NetBoot(ExeDirectory);
		var s = new NexusSettings(b);
		
		System.Windows.Forms.Application.Run(new IamForm(new Iam(s)));
	}

	public Iam(NexusSettings settings)
	{
		Nexus = new NexusApiClient(ApiClient.GetAddress(settings.Zone, settings.Api.LocalIP, false, KnownSystem.NexusApi), null);
		Vault = new VaultApiClient(ApiClient.GetAddress(settings.Zone, settings.Api.LocalIP, false, KnownSystem.VaultApi), null);
	}
}