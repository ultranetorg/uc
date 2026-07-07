using Uccs.Net;

namespace Uccs.Nexus.CLI;

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
		Thread.CurrentThread.CurrentCulture = 
		Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

		new NexusCli();
	}
}	
