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

		a.Description = "Handles entity by its Unel address";
		a.Arguments =	[
							new (null, UNEL, "Address to open"),
						];

		a.Execute = () =>	{
								//Open.Start(Unel.Parse(Args[0].Name), Flow);

								return null;
							};
		return a;
	}
}