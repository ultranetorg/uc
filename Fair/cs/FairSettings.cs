using System.Diagnostics;

namespace Uccs.Fair
{
	public class SeedSettings : Settings
	{
		public string	Releases { get; set; }
		public int		CollectRefreshInterval { get; set; } = 60000;

		public SeedSettings() : base(FairXonTextValueSerializator.Default)
		{
		}
	}

	public class SeedHubSettings : Settings
	{
		public SeedHubSettings() : base(FairXonTextValueSerializator.Default)
		{
		}
	}

	public class EthereumSettings : Settings
	{
		public string		Provider { get; set; }

		public EthereumSettings() : base(FairXonTextValueSerializator.Default)
		{
		}
	}

	public class FairNodeSettings : SavableSettings
	{
		public McvSettings				Mcv { get; set; }
		public PeeringSettings			Peering { get; set; } = new();
		public ApiSettings				Api { get; set; }
		public bool						Log { get; set; }
		public int						RdcQueryTimeout { get; set; } = 5000;
		public int						RdcTransactingTimeout { get; set; } = 5*60*1000;

		public long						Roles => Mcv?.Roles ?? 0;


		public FairNodeSettings() : base(NetXonTextValueSerializator.Default)
		{
		}

		public FairNodeSettings(string profile) : base(profile, NetXonTextValueSerializator.Default)
		{
			if(Debugger.IsAttached)
			{
				RdcQueryTimeout = int.MaxValue;
				RdcTransactingTimeout = int.MaxValue;
			}
		}
	}
}
