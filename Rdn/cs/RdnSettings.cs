using System.Diagnostics;

namespace Uccs.Rdn;

public class SeedSettings : Settings
{
	public string	Releases { get; set; }
	public int		CollectRefreshInterval { get; set; } = 60000;

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

	public string					GoogleSearchEngineID { get; set; }
	public string					GoogleApiKey { get; set; }

	public SeedSettings				Seed { get; set; }
	public SeedHubSettings			SeedHub { get; set; } = new ();

	public new long					Roles => (Mcv?.Roles ?? 0) | (Seed != null ? (long)RdnRole.Seed : 0);


	public RdnNodeSettings()
	{
	}

	public RdnNodeSettings(string profile) : base(profile)
	{
		if(Debugger.IsAttached)
		{
			RdcQueryTimeout = int.MaxValue;
			RdcTransactingTimeout = int.MaxValue;
		}
	}
}
