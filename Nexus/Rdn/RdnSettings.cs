using System.Diagnostics;
using System.Net;

namespace Uccs.Rdn;

public class SeedSettings : Settings
{
	public string	Releases { get; set; }
	public int		RefreshInterval { get; set; } = 60000;

	public SeedSettings() : base(RdnXonTextValueSerializator.Default)
	{
	}
}

public class SeedHubSettings : Settings
{
	public SeedHubSettings() : base(RdnXonTextValueSerializator.Default)
	{
	}
}

public class EthereumSettings : Settings
{
	public string		Provider { get; set; }

	public EthereumSettings() : base(RdnXonTextValueSerializator.Default)
	{
	}
}

public class RdnNodeSettings : McvNodeSettings
{
	public List<AccountAddress>		ProposedFundJoiners = new();
	public List<AccountAddress>		ProposedFundLeavers = new();

	public string[]					ProposedFriendAttachments { get; set; } = [];

	public SeedSettings				Seed { get; set; }
	public SeedHubSettings			SeedHub { get; set; } = new ();

	public new long					Roles => (Mcv?.Roles ?? 0) | (Seed != null ? (long)RdnRole.Seed : 0);

	public string					DataPath { get; set; }

	public RdnNodeSettings()
	{
	}

	public RdnNodeSettings(string profile) : base(profile)
	{
		if(Debugger.IsAttached)
		{
			PpcTimeout = int.MaxValue;
			TransactingTimeout = int.MaxValue;
		}
	}

	public RdnNodeSettings(string profile, Zone zone, NexusSettings nexusSettings) : base(profile)
	{
		if(!nexusSettings.Exists)
			throw new Exception("NexusSettings not found");

		if(!Exists)
		{
			SetDefaults(zone, nexusSettings);
			Save();
		}
	}

	public void SetDefaults(Zone zone, NexusSettings settings)
	{
		Peering		= new () {Endpoint = new (IPAddress.Any, Rdn.ByZone(zone).PpiPort)};
		Api			= new () {LocalIP = settings.Host};
		DataPath	= System.IO.Path.Join(RdnNode.ExeDirectory, "Data");
	}

}
