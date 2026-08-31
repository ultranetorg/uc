using System.Diagnostics;
using System.Net;

namespace Uccs.Fair;

public class SeedSettings : Settings
{
	public string	Releases { get; set; }
	public int		CollectRefreshInterval { get; set; } = 60000;

	public SeedSettings() : base(FairXonTextValueSerializator.Default)
	{
	}
}

public class SeedHubSettings : Settings
{
	public SeedHubSettings() : base(FairXonTextValueSerializator.Default)
	{
	}
}

public class WebSettings : Settings
{
	public string		ListenUrl { get; set; }
	public bool			Logging { get; set; }

	public WebSettings() : base(FairXonTextValueSerializator.Default)
	{
	}
}

public class FairNodeSettings : McvNodeSettings
{
	public WebSettings		Web { get; set; }
	public string			DataPath { get; set; }

	public FairNodeSettings()
	{
	}

	public FairNodeSettings(string uosprofile) : base(uosprofile)
	{
		if(Debugger.IsAttached)
		{
			PpcTimeout = int.MaxValue;
			TransactingTimeout = int.MaxValue;
		}
	}

	public FairNodeSettings(NexusSettings nexusSettings) : base(System.IO.Path.Join(nexusSettings.Profile, FairNode.UniqueDirectiryName))
	{
		if(!nexusSettings.Exists)
			throw new Exception("NexusSettings not found");

		if(!Exists)
		{
			SetDefaults(nexusSettings);
			Save();
		}
	}

	public void SetDefaults(NexusSettings settings)
	{
		Peering		= new () {Endpoint = new (IPAddress.Any, Fair.ByZone(settings.Zone).PpiPort)};
		Api			= new () {LocalIP = settings.Host};
		DataPath	= System.IO.Path.Join(FairNode.ExeDirectory, "Data");
	}
}
