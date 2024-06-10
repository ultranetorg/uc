using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uccs.Net;

namespace Uccs.Uos
{
	public class UosSettings : Settings
	{
		public const string				FileName = "Uos.settings";
		public Zone						Zone;
		public string					Name;
		public ApiSettings				Api { get; set; }
		public Guid						CliDefaultMcv { get; set; } = Rdn.Id;

		public UosSettings() : base(NetXonTextValueSerializator.Default)
		{
		}

		public UosSettings(string profile, string name, Zone zone) : base(profile, FileName, NetXonTextValueSerializator.Default)
		{
			Name = name;
			Zone = zone;
		}
	}
}
