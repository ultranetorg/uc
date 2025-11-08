using System.Net;

namespace Uccs.Vault;

public class VaultSettings : SavableSettings
{
	public Zone				Zone;
	public IPAddress		Host { get; set; } = Net.Net.DefaultHost;
	public bool				Encrypt { get; set; }
	public IpApiSettings	Api { get; set; }
	public byte[]			AdminKey;

	public VaultSettings() : base(NetXonTextValueSerializator.Default)
	{
	}

	public VaultSettings(string profile, Zone zone, Xon command) : base(profile, NetXonTextValueSerializator.Default)
	{
		Zone = zone;

		AdminKey = command?.One("VaultAdminKey")?.Get<byte[]>();

	}
}
