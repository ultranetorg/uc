using System.Reflection;

namespace Uccs.Fair;

public class NodeCommand : Uccs.Net.NodeCommand
{
	public NodeCommand(McvCli cli, List<Xon> args, Flow flow) : base(cli, args, flow)
	{
	}

	protected override McvApiClient CreateClient(string url)
	{
		return new FairApiClient(url, GetString(Apc.AccessKey, null));
	}

	public CommandAction Run()
	{
 		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Runs a new instance with command-line interface";
		a.Arguments =	[
							new ("profile", DIRPATH, "Path to local profile directory"),
						];

		a.Execute = () =>	{
								Cli.Node = new FairNode(Cli.Net.Zone, 
														GetString("profile", Cli.Boot.Profile), 
														Cli.NexusSettings, 
														Cli.Settings as FairNodeSettings, 
														new RealClock(), 
														new Flow(Flow, new Log())); /// Use the same Cancellation to allow to exit by API call or other
								
								Cli.InteractOrWait(Cli.Boot.Profile, this, a, Cli.Node.Flow);

								if(Cli.Node.Flow.Active)
									Cli.Node.Stop();

								Cli.Node = null;

								return null;
							};
		
		return a;
	}
}

