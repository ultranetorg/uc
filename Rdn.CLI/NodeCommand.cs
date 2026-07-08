using System.Reflection;

namespace Uccs.Rdn;

public class NodeCommand : Net.NodeCommand
{
	public NodeCommand(McvCli cli, List<Xon> args, Flow flow) : base(cli, args, flow)
	{
	}

	protected override McvApiClient CreateClient(string url)
	{
		return new RdnApiClient(url, GetString(Apc.AccessKey, null));
	}

//	public CommandAction Run()
//	{
// 		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
//
//		a.Name = "r";
//		a.Description = "Runs a new instance with command-line interface";
//		a.Arguments =	[
//							new ("profile", DIRPATH, "Path to local profile directory"),
//						];
//
//		a.Execute = () =>	{
//								Cli.Node = new RdnNode(	Cli.Net.Zone, 
//														GetString("profile", Cli.Boot.Profile), 
//														Cli.NexusSettings, 
//														Cli.Settings as RdnNodeSettings, 
//														new RealClock(), 
//														Flow);
//
//								Cli.InteractOrWait(Cli.Boot.Profile, this, a, Flow);
//
//								if(Cli.Node.Flow.Active)
//									Cli.Node.Stop();
//
//								Cli.Node = null;
//
//								return null;
//							};
//		
//		return a;
//	}
}

