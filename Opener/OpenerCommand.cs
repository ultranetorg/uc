using System.Reflection;
using Uccs.Net;

namespace Uccs.Opener;

public class OpenerCommand : NetCommand
{
	Opener Opener;

	public OpenerCommand(Opener opener, List<Xon> args, Flow flow) : base(args, flow)
	{
		Opener = opener;
	}

	public CommandAction Default()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Execute = () =>	{
								Opener.Start(Unea.Parse(Args[0].Name), Flow);

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