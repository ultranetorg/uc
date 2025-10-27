using System.Net;

namespace Uccs.Vault;

public class VaultSettings : SavableSettings
{
	public Zone				Zone;
	public IPAddress		Host { get; set; } = Net.Net.DefaultHost;
	public bool				Encrypt { get; set; }
	public IpApiSettings	Api { get; set; }

	public VaultSettings() : base(NetXonTextValueSerializator.Default)
	{
	}

	public VaultSettings(string profile, Zone zone) : base(profile, NetXonTextValueSerializator.Default)
	{
		Zone = zone;
	}
}
