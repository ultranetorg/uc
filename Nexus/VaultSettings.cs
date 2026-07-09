using System.Net;

namespace Uccs.Nexus;

public class VaultSettings : SavableSettings
{
	public IpApiSettings	Api { get; set; }
	public bool				CreateFirstAccountIfEmpty { get; set; } = true;

	public VaultSettings() : base(NetXonTextValueSerializator.Default)
	{
	}

	public VaultSettings(NexusSettings settings) : base(settings.Profile, NetXonTextValueSerializator.Default)
	{
		if(!File.Exists(Path))
		{
			Api	= new () {LocalIP = settings.Host};
			
			Save();
		}
	}
}
