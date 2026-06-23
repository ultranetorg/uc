using System.Net;

namespace Uccs.Vault;

public class VaultSettings : SavableSettings
{
	public IpApiSettings	Api { get; set; }
	public bool				CreateFirstAccountIfEmpty { get; set; } = true;

	public VaultSettings() : base(NetXonTextValueSerializator.Default)
	{
	}

	public VaultSettings(string profile) : base(profile, NetXonTextValueSerializator.Default)
	{
	}
}
