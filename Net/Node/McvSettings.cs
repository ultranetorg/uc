using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class McvSettings : SettingsBase
	{
		public PeeringSettings			Peering { get; set; }= new();
		public Money					Bail { get; set; }
		public AccountAddress[]			Generators { get; set; } = [];
		public string					GoogleSearchEngineID { get; set; }
		public string					GoogleApiKey { get; set; }
		//public List<AccountAddress>	ProposedFundJoiners = new();
		//public List<AccountAddress>	ProposedFundLeavers = new();
		public Role						Roles { get; set; }

		public McvSettings() : base(NetXonTextValueSerializator.Default)
		{
		}

		public McvSettings(string exedir, string profile, string filename) : base(exedir, profile, filename, NetXonTextValueSerializator.Default)
		{
		}
	}
}
