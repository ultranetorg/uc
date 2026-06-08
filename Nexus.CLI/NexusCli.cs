using Uccs.Net;

namespace Uccs.Nexus.CLI;

public class NodeInstance
{
	public string		ApiLocalAddress { get; set; }
	public string		Net;

	public override string ToString()
	{
		return Net;
	}
}

public class NexusCli : Cli
{
	public Nexus				Nexus;
	public NetBoot				Boot;
	public NexusSettings		Settings;

	NexusApiClient				_Nexus;
	public NexusApiClient		NexusApi => _Nexus ??= new NexusApiClient(Settings.Api.LocalSystemAddress(Settings.Zone, Api.Nexus));

	static NexusCli()
	{
	}

	public NexusCli()
	{
		Boot = new NetBoot(ExeDirectory);
		Settings = new NexusSettings(Boot.Zone, Boot.Profile);

		Execute(Boot);
	}

	public NexusCli(Nexus nexus)
	{
		Nexus = nexus;
	}

	public static void Main(string[] args)
	{
		new NexusCli();
	}
}	
