using System.Reflection;

namespace Uccs.Vault;

public class VaultCommand : NetCommand
{
	public Vault	Vault;

	public VaultCommand(Vault vault, List<Xon> args, Flow flow) : base(args, flow)
	{
		Vault = vault;
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
								Run(Vault, a);

								return null;
							};
		
		return a;
	}
}
