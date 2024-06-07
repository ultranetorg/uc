using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class SeedHubSettings : Settings
	{
		public SeedHubSettings() : base(NetXonTextValueSerializator.Default)
		{
		}
	}

	public class ResourceHubSettings : Settings
	{
		public int		CollectRefreshInterval { get; set; } = 60000;

		public ResourceHubSettings() : base(NetXonTextValueSerializator.Default)
		{
		}
	}

	public class EthereumSettings : Settings
	{
		public string		Provider { get; set; }

		public EthereumSettings() : base(NetXonTextValueSerializator.Default)
		{
		}
	}

	public class RdsSettings : McvSettings
	{
		public const string				FileName = "Rds.settings";

		public List<AccountAddress>		ProposedFundJoiners = new();
		public List<AccountAddress>		ProposedFundLeavers = new();
		public string					Packages { get; set; }
		public string					Releases { get; set; }

		public EthereumSettings			Ethereum { get; set; } = new ();
		public SeedHubSettings			SeedHub { get; set; } = new ();
		public ResourceHubSettings		ResourceHub { get; set; } = new ();

		public RdsSettings()
		{
		}

		public RdsSettings(string profile) : base(profile, FileName)
		{
		}
	}
}
