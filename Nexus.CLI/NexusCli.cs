using System.Reflection;
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

		//var c = Console.ForegroundColor;
		//Console.Write($"Zone    = ");
		//Console.ForegroundColor = ConsoleColor.Green;
		//Console.WriteLine($"{Boot.Zone}");
		//Console.ForegroundColor = c;
		//Console.Write($"Profile = ");
		//Console.ForegroundColor = ConsoleColor.Green;
		//Console.WriteLine(Boot.Profile);
		//Console.ForegroundColor = c;

		Execute(Boot.Profile, Boot.Commnand);
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

	public override Command Create(IEnumerable<Xon> commnad, Flow flow)
	{
		return CreateFromAssembly(Assembly.GetExecutingAssembly(), commnad, flow);
	}
}	
