namespace Uccs.Rdn
{
	public class SeedSettings : Settings
	{
		public string	Packages { get; set; }
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

	public class RdnSettings : McvSettings
	{
		public List<AccountAddress>		ProposedFundJoiners = new();
		public List<AccountAddress>		ProposedFundLeavers = new();

		public string					GoogleSearchEngineID { get; set; }
		public string					GoogleApiKey { get; set; }

		public SeedSettings				Seed { get; set; }
		public EthereumSettings			Ethereum { get; set; } = new ();
		public SeedHubSettings			SeedHub { get; set; } = new ();

		public override long			Roles => base.Roles | (Seed != null ? (long)RdnRole.Seed : 0);


		public RdnSettings()
		{
		}

		public RdnSettings(string profile) : base(profile)
		{
		}
	}
}
