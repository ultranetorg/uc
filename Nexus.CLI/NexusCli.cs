using System.Reflection;
using Uccs.Net;

namespace Uccs.Nexus.CLI;

public class NexusCli : NetCli
{
	public Nexus					Nexus;
	public VaultSettings			VaultSettings;

	NexusApiClient					_Api;
	public override JsonApiClient	Api => _Api ??= new NexusApiClient(NexusSettings.Api.LocalSystemAddress(NexusSettings.Zone, Net.Api.Nexus));

	VaultApiClient					_VaulApi;
	public JsonApiClient			VaultApi => _VaulApi ??= new VaultApiClient(NexusSettings.Api.LocalSystemAddress(Boot.Zone, Net.Api.Vault));

	static NexusCli()
	{
	}

	public NexusCli()
	{
		Boot = new NetBoot(ExeDirectory);
		NexusSettings = new NexusSettings(Boot.Zone, Boot.Profile);
		VaultSettings = new VaultSettings(NexusSettings);

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
