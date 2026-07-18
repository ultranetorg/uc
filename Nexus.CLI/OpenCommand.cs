using System.Reflection;
using Uccs.Net;

namespace Uccs.Nexus.CLI;

public class OpenCommand : NexusCommand
{
	public OpenCommand(NexusCli cli, List<Xon> args, Flow flow) : base(cli, args, flow)
	{
	}

	public CommandAction Default()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Handles entity by the corresponding net client software. Runs a new instance of the client or uses existing. Downloads default client if necessary. Default nodes are retrieved from the parent network.";
		a.Arguments =	[
							new (null, SNQ, "Address to open"),
						];

		a.Execute = () =>	{
								Cli.Api.Send(new DoApc {Query = Snq.Parse(First)}, Flow);

								return null;
							};
		return a;
	}
}