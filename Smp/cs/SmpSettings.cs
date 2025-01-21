using System.Diagnostics;

namespace Uccs.Smp;

public class SeedSettings : Settings
{
	public string	Releases { get; set; }
	public int		CollectRefreshInterval { get; set; } = 60000;

	public SeedSettings() : base(SmpXonTextValueSerializator.Default)
	{
	}
}

public class SeedHubSettings : Settings
{
	public SeedHubSettings() : base(SmpXonTextValueSerializator.Default)
	{
	}
}

public class EthereumSettings : Settings
{
	public string		Provider { get; set; }

	public EthereumSettings() : base(SmpXonTextValueSerializator.Default)
	{
	}
}

public class SmpNodeSettings : McvNodeSettings
{
	public string		WebServerListenUrl { get; set; }

	public SmpNodeSettings()
	{
	}

	public SmpNodeSettings(string profile) : base(profile)
	{
		if(Debugger.IsAttached)
		{
			RdcQueryTimeout = int.MaxValue;
			RdcTransactingTimeout = int.MaxValue;
		}
	}
}
