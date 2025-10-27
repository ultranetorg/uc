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

		a.Execute = () =>	{
								Open.Start(Unea.Parse(Args[0].Name), Flow);

								return null;
							};

		a.Help = new()
				 {
					Description = "",
					Syntax = $"{Keyword} {Rdn.CLI.RdnCommand.RA}",

					Arguments =	[
									new (FirstArg, "Address to open"),
								],

					Examples =	[
									new (null, $"{Keyword} {Rdn.CLI.RdnCommand.RA.Example}")
								]
				 };
		return a;
	}
}