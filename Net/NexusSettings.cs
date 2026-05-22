using System.Net;

namespace Uccs.Net;


public class NexusSessionSettings
{
	public string			Net { get; set; }
	public AccountAddress	Signer { get; set; }
	public byte[]			Session { get; set; }
}

public class NexusSettings : SavableSettings
{
	public string					Name { get; set; }
	public IPAddress				Host { get; set; }
	public IpApiSettings			Api { get; set; }
	public string					Packages { get; set; }
	public NexusSessionSettings[]	Sessions { get; set; }
	public PeeringSettings			IccpPeering { get; set; }

	public Zone						Zone;

	public NexusSettings() : base(NetXonTextValueSerializator.Default)
	{
	}

	public NexusSettings(Zone zone, string profile) : base(profile, NetXonTextValueSerializator.Default)
	{
		Zone = zone;
	}
}
