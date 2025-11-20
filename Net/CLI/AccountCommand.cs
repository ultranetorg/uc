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

		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "c";
		a.Description = "Create a new account entity";
		a.Arguments = [new (owner, AA, "Account public address of the assigned account owner", Flag.First)];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);

								return new AccountCreation {Owner = GetAccountAddress(owner)};
							};
		return a;
	}

	public virtual CommandAction Entity()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "e";
		a.Description = "Get account entity information from MCV database";
		a.Arguments = [new (null, AA,  "Address of an account to get information about", Flag.First)];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);

								var i = Ppc(new AccountPpc(First));
												
								Flow.Log.Dump(i.Account);

								return i.Account;
							};
		return a;
	}
}
