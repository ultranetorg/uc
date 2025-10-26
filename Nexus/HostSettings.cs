using System.Net;
using Uccs.Net;

namespace Uccs.Uos;

public class HostSettings : SavableSettings
{
	public string			Name;
	public Zone				Zone;
	public IpApiSettings	Api { get; set; }
	public string			Packages { get; set; }

	public HostSettings() : base(NetXonTextValueSerializator.Default)
	{
	}

	public HostSettings(string profile, string name, Zone zone) : base(profile, NetXonTextValueSerializator.Default)
	{
		Name = name;
		Zone = zone;
	}
}
