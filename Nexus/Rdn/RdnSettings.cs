using System.Diagnostics;
using System.Net;

namespace Uccs.Rdn;

public class SeedSettings : Settings
{
	public string	Releases { get; set; }
	public int		RefreshInterval { get; set; } = 60000;

	public SeedSettings() : base(RdnXonTextValueSerializator.Default)
	{
	}
}

public class SeedHubSettings : Settings
{
	public SeedHubSettings() : base(RdnXonTextValueSerializator.Default)
	{
	}
}

public class EthereumSettings : Settings
{
	public string		Provider { get; set; }

	public EthereumSettings() : base(RdnXonTextValueSerializator.Default)
	{
	}
}

public class RdnNodeSettings : McvNodeSettings
{
	public List<PublicKey>		ProposedFundJoiners = new();
	public List<PublicKey>		ProposedFundLeavers = new();

	public List<string>			ProposedFriendAttachments { get; set; } = [];

	public SeedSettings			Seed { get; set; }
	public SeedHubSettings		SeedHub { get; set; } = new ();

	public new long				Roles => (Mcv?.Roles ?? 0) | (Seed != null ? (long)RdnRole.Seed : 0);

	public string				DataPath { get; set; }
	public const string			DataRelativePath = "Data";

	public RdnNodeSettings()
	{
	}

	public RdnNodeSettings(string profile) : base(profile)
	{
		if(Debugger.IsAttached)
		{
			PpcTimeout = int.MaxValue;
			TransactingTimeout = int.MaxValue;
		}
	}

	public RdnNodeSettings(NexusSettings nexusSettings) : base(System.IO.Path.Join(nexusSettings.Profile, RdnNode.UniqueDirectiryName))
	{
		if(!nexusSettings.Exists)
			throw new Exception("NexusSettings not found");

		if(!Exists)
		{
			SetDefaults(nexusSettings);
			Save();
		}
		
		DataPath ??= System.IO.Path.Join(RdnNode.ExeDirectory, DataRelativePath);
	}

	public void SetDefaults(NexusSettings settings)
	{
		Peering		= new () {Endpoint = new (settings.Host, Rdn.ByZone(settings.Zone).PpiPort)};
		Api			= new () {LocalIP = settings.Host};
		Seed		= new();
	}
}
