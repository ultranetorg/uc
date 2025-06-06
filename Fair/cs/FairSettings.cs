﻿using System.Diagnostics;

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

public class EthereumSettings : Settings
{
	public string		Provider { get; set; }

	public EthereumSettings() : base(FairXonTextValueSerializator.Default)
	{
	}
}

public class FairNodeSettings : McvNodeSettings
{
	public string		WebServerListenUrl { get; set; }
	public bool			WebServerLogging { get; set; }

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
