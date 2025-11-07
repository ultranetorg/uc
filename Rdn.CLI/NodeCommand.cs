using System.Reflection;

namespace Uccs.Rdn;

public class NodeCommand : Net.NodeCommand
{
	public NodeCommand(McvCli cli, List<Xon> args, Flow flow) : base(cli, args, flow)
	{
	}

	public CommandAction Run()
	{
 		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "r";
		a.Description = "Runs a new instance with command-line interface";
		a.Arguments =	[
							new ("name", NAME, "An arbitrary name of a node"),
							new ("zone", ZONE, "Zone name"),
							new ("profile", DIRPATH, "Path to local profile directory"),
						];

		a.Execute = () =>	{
								Cli.Node = new RdnNode(GetString("name", null), GetEnum("zone", Cli.Net.Zone), GetString("profile", Cli.Settings.Profile), Cli.Settings as RdnNodeSettings, new RealClock(), Flow);

								Cli.Run(this, a);

								Cli.Node.Stop();

								return null;
							};
		
		return a;
	}
}

