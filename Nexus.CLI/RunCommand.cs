using System.Reflection;
using Uccs.Net;
using Uccs.Vault;

namespace Uccs.Nexus.CLI;

internal class RunCommand : NexusCommand
{
	public RunCommand(NexusCli uos, List<Xon> args, Flow flow) : base(uos, args, flow)
	{
	}

	public CommandAction Default()
	{
 		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Runs a new instance with command-line interface";
		a.Arguments =	[
							new ("profile", DIRPATH, "Path to local profile directory", Flag.Optional),
							new ("zone", ZONE, "Zone name", Flag.Optional),
						];

		a.Execute = () =>	{
								var b = Cli.Boot;

								var ns = new NexusSettings(b.Zone, b.Profile);
								var vs = new VaultSettings(b.Profile);

								Cli.Nexus = new Nexus(b, ns, vs, new Flow(nameof(Nexus), new Log()){WorkDirectory = b.Profile});
								Cli.Nexus.RunRdn(null, new RealClock());

								Cli.Run(this, a);

								Cli.Nexus.Stop();
								return null;
							};
		
		

		return a;
	}
}