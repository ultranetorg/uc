using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uccs.Net;

namespace Uccs.Uos
{
	public class UosSettings : SettingsBase
	{
		public const string				FileName = "Uos.settings";
		public ApiSettings				Api { get; set; }
		public Zone						Zone { get; set; }
		public string					Name { get; set; }
		public Guid						CliDefaultMcv { get; set; } = Rds.Id;

		public UosSettings() : base(NetXonTextValueSerializator.Default)
		{
		}

		public UosSettings(string exedir, string profile, string name, Zone zone) : base(exedir, profile, FileName, NetXonTextValueSerializator.Default)
		{
			Name = name;
			Zone = zone;
		}
	}
}
