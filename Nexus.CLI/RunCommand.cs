using System.Reflection;

namespace Uccs.Nexus;

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
								Cli.Run(this, a);
								return null;
							};
		
		

		return a;
	}
}