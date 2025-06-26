using System.Diagnostics;

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
	public WebSettings	Web { get; set; }

	public FairNodeSettings()
	{
	}

	public FairNodeSettings(string profile) : base(profile)
	{
		if(Debugger.IsAttached)
		{
			RdcQueryTimeout = int.MaxValue;
			RdcTransactingTimeout = int.MaxValue;
		}
	}
}
