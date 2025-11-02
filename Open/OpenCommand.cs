using System.Reflection;
using Uccs.Net;

namespace Uccs.Open;

public class OpenCommand : NetCommand
{
	Open Open;

	public OpenCommand(Open opener, List<Xon> args, Flow flow) : base(args, flow)
	{
		Open = opener;
	}

	public CommandAction Default()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Help = new()
				 {
					Description = "Handles entity by its Unel address",
					Arguments =	[
									new (null, UNEL, "Address to open"),
								]
				};

		a.Execute = () =>	{
								Open.Start(Unel.Parse(Args[0].Name), Flow);

								return null;
							};
		return a;
	}
}