using System.Reflection;

namespace Uccs.Vault;

public class VaultCommand : NetCommand
{
	public Vault					Vault;

	public VaultCommand(Vault uos, List<Xon> args, Flow flow) : base(args, flow)
	{
		Vault = uos;
	}

	public CommandAction Run()
	{
 		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "r";
		a.Help = new() {Description = "Runs a new instance with command-line interface",
						Syntax = $"{Keyword} {a.NamesSyntax} flags [profile={DIRPATH}]",

						Arguments =	[
										new ("profile", "Path to local profile directory"),
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} profile=C:\\NodeProfile")
									]};

		a.Execute = () =>	{
								Run(Vault, a);

								return null;
							};
		
		return a;
	}
}
