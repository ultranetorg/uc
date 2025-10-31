using Uccs.Net;

namespace Uccs.Nexus;

public class NexusSettings : SavableSettings
{
	public string			Name { get; set; }
	public IpApiSettings	Api { get; set; }
	public string			Packages { get; set; }
	public Zone				Zone;

	public NexusSettings(NetBoot boot) : base(boot.Profile, NetXonTextValueSerializator.Default)
	{
		Zone = boot.Zone;
	}
}
