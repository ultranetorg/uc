using System.Reflection;

namespace Uccs.Vault.CLI;

public class VaultCommand : NetCommand
{
	public VaultCli	Cli;

	public VaultCommand(VaultCli vault, List<Xon> args, Flow flow) : base(args, flow)
	{
		Cli = vault;
	}

	public CommandAction Run()
	{
 		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "r";
		a.Description = "Runs a new instance with command-line interface";
		a.Arguments =	[
							new ("profile", DIRPATH, "Path to local profile directory", Flag.Optional),
						];

		a.Execute = () =>	{
								Run(Cli, a);

								return null;
							};
		
		return a;
	}
}
