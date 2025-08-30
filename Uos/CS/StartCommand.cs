using System.Reflection;

namespace Uccs.Uos;

public class StartCommand : UosCommand
{
	public StartCommand(Uos uos, List<Xon> args, Flow flow) : base(uos, args, flow)
	{
	}

	public CommandAction Default()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Execute = () =>	{
								Uos.Start(Unea.Parse(Args[0].Name), Flow);

								return null;
							};

		a.Help = new() {Description = "",
						Syntax = $"{Keyword} {Rdn.CLI.RdnCommand.RA}",

						Arguments =	[
										new (FirstArg, "Address of resource to execute"),
									],

						Examples =	[
										new (null, $"{Keyword} {Rdn.CLI.RdnCommand.RA.Example}")
									]};
		return a;
	}
}