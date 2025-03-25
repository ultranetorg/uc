using System.Reflection;

namespace Uccs.Net;

public class AccountCommand : McvCommand
{
	protected AccountIdentifier		First => AccountIdentifier.Parse(Args[0].Name);

	public AccountCommand(McvCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Create()
	{
		var owner = "owner";

		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "c";
		a.Help = new() {Description = "Create a new account entity",
						Syntax = $"{Keyword} {a.NamesSyntax} {owner}={AA}",

						Arguments = [new ("owner", "Account public address of the assigned account owner")],

						Examples = [new (null, $"{Keyword} {a.Name} {owner}={AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);

								return new AccountCreation {Owner = GetAccountAddress(owner)};
							};
		return a;
	}

	public virtual CommandAction Entity()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "e";
		a.Help = new() {Description = "Get account entity information from Ultranet distributed database",
						Syntax = $"{Keyword} {a.NamesSyntax} {AA}",

						Arguments = [new ("<first>", "Address of an account to get information about")],

						Examples = [new (null, $"{Keyword} {a.Name} {AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);

								var i = Ppc(new AccountRequest(First));
												
								Dump(i.Account);

								return i.Account;
							};
		return a;
	}
}
