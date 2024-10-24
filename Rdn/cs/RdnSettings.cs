using System.Diagnostics;

namespace Uccs.Rdn
{
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

	public class RdnNodeSettings : SavableSettings
	{
		public McvSettings				Mcv { get; set; }
		public PeeringSettings			Peering { get; set; } = new();
		public PeeringSettings			NtnPeering { get; set; }
		public ApiSettings				Api { get; set; }
		public bool						Log { get; set; }
		public int						RdcQueryTimeout { get; set; } = 5000;
		public int						RdcTransactingTimeout { get; set; } = 5*60*1000;

		public List<AccountAddress>		ProposedFundJoiners = new();
		public List<AccountAddress>		ProposedFundLeavers = new();

		public string					GoogleSearchEngineID { get; set; }
		public string					GoogleApiKey { get; set; }

		public SeedSettings				Seed { get; set; }
		public SeedHubSettings			SeedHub { get; set; } = new ();

		public long						Roles => (Mcv?.Roles ?? 0) | (Seed != null ? (long)RdnRole.Seed : 0);


		public RdnNodeSettings() : base(NetXonTextValueSerializator.Default)
		{
		}

		public RdnNodeSettings(string profile) : base(profile, NetXonTextValueSerializator.Default)
		{
			if(Debugger.IsAttached)
			{
				RdcQueryTimeout = int.MaxValue;
				RdcTransactingTimeout = int.MaxValue;
			}
		}
	}
}
