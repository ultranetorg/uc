using System.Net;
using Uccs.Net;

namespace Uccs.Uos;

public class UosSettings : SavableSettings
{
	public string			Name;
	public Zone				Zone;
	public IpApiSettings	Api { get; set; }
	public string			Packages { get; set; }

	public UosSettings() : base(NetXonTextValueSerializator.Default)
	{
	}

	public UosSettings(string profile, string name, Zone zone) : base(profile, NetXonTextValueSerializator.Default)
	{
		Name = name;
		Zone = zone;
	}
}
