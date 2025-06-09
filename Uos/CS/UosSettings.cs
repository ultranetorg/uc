using Uccs.Net;

namespace Uccs.Uos;

public class UosSettings : SavableSettings
{
	public string			Name;
	public Rdn.Rdn			Rdn;
	public bool				EncryptVault { get; set; }
	public ApiSettings		Api { get; set; }
	public int				NodesApiListenPortPostfix { get; set; }
	public string			Packages { get; set; }

	public UosSettings() : base(NetXonTextValueSerializator.Default)
	{
	}

	public UosSettings(string profile, string name, Rdn.Rdn root) : base(profile, NetXonTextValueSerializator.Default)
	{
		Name = name;
		Rdn = root;
	}
}
