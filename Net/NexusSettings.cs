using System.Net;

namespace Uccs.Net;

public class NexusSettings : SavableSettings
{
	public string			Name { get; set; }
	public IPAddress		Host { get; set; } = Net.DefaultHost;
	public IpApiSettings	Api { get; set; }
	public string			Packages { get; set; }
	public PeeringSettings	NnPeering { get; set; }

	public Zone				Zone;

	public NexusSettings() : base(NetXonTextValueSerializator.Default)
	{
	}

	public NexusSettings(Zone zone, string profile) : base(profile, NetXonTextValueSerializator.Default)
	{
		Zone = zone;
	}
}
