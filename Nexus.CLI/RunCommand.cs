using System.Reflection;
using Uccs.Net;

namespace Uccs.Nexus.CLI;

public class RunCommand : NexusCommand
{
	public RunCommand(NexusCli uos, List<Xon> args, Flow flow) : base(uos, args, flow)
	{
	}

	public CommandAction Default()
	{
 		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Runs a new instance with command-line interface";
		a.Arguments =	[
							new ("profile", DIRPATH, "Path to local profile directory", ArgumentFlag.Optional),
							new ("zone", ZONE, "Zone name", ArgumentFlag.Optional),
						];

		a.Execute = () =>	{
								var b = Cli.Boot;

								var ns = new NexusSettings(b.Zone, b.Profile);
								var vs = new VaultSettings(ns);

								Cli.Nexus = new Nexus(b, ns, vs, new Flow(nameof(Nexus), new Log()){WorkDirectory = b.Profile});
								Cli.Nexus.RunRdn(null, new RealClock());

								Cli.InteractOrWait(b.Profile, this, a, Cli.Nexus.Flow);

								Cli.Nexus.Stop();

								return null;
							};

		return a;
	}
}