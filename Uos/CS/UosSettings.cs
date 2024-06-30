using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uccs.Net;

namespace Uccs.Uos
{
	public class UosSettings : SavableSettings
	{
		public string					Name;
		public Zone						Interzone;
		public bool						EncryptVault { get; set; }
		public ApiSettings				Api { get; set; }

		public UosSettings() : base(NetXonTextValueSerializator.Default)
		{
		}

		public UosSettings(string profile, string name, Zone interzone) : base(profile, NetXonTextValueSerializator.Default)
		{
			Name = name;
			Interzone = interzone;
		}
	}
}
