using System.Net;

namespace Uccs.Vault;

public class VaultSettings : SavableSettings
{
	//public IPAddress		Host { get; set; } = Net.Net.DefaultHost;
	//public bool				Encrypt { get; set; }
	public IpApiSettings	Api { get; set; }

	public VaultSettings() : base(NetXonTextValueSerializator.Default)
	{
	}

	public VaultSettings(string profile) : base(profile, NetXonTextValueSerializator.Default)
	{
	}
}
