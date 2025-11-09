#if false
using System.Reflection;
using Uccs.Net;
using Uccs.Nexus;

namespace Uccs.Nexus.Windows;

public class IamSettings : SavableSettings
{
	public byte[]			VaultAdminKey;
	public Zone				Zone;

	public IamSettings(NetBoot boot) : base(boot.Profile, NetXonTextValueSerializator.Default)
	{
		Zone			= boot.Zone;
		VaultAdminKey	= boot.Commnand.Get<byte[]>("VaultAdminKey", null);
	}

	public IamSettings(string profile) : base(profile, NetXonTextValueSerializator.Default)
	{
	}
}

public class Iam
{
	public static string		ExeDirectory;
	public NexusApiClient		Nexus;
	public VaultApiClient		Vault;
	public IamSettings			Settings;

	[STAThread]
	static void Main()
	{
		ApplicationConfiguration.Initialize();
		//Application.SetColorMode(SystemColorMode.Dark);
		
		ExeDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
				
		var b = new NetBoot(ExeDirectory);
		var ns = new NexusSettings(b);
		var s = new IamSettings(b);
		
		System.Windows.Forms.Application.Run(new IamForm(new Iam(ns, s)));
	}

	public Iam(NexusSettings nexussettings, IamSettings iamsettings)
	{
		Settings = iamsettings;

		Nexus = new NexusApiClient(ApiClient.GetAddress(iamsettings.Zone, nexussettings.Api.LocalIP, false, KnownSystem.NexusApi), null);
		Vault = new VaultApiClient(ApiClient.GetAddress(iamsettings.Zone, nexussettings.Api.LocalIP, false, KnownSystem.VaultApi), null);
	}
}
#endif

