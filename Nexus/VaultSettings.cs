using System.Net;

namespace Uccs.Nexus;

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
