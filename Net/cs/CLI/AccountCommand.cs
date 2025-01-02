using System.Reflection;

namespace Uccs.Net;

public class AccountCommand : McvCommand
{
	AccountIdentifier		First => AccountIdentifier.Parse(Args[0].Name);

	public AccountCommand(McvCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Entity()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "e";
		a.Help = new() {Description = "Get account entity information from Ultranet distributed database",
						Syntax = $"{Keyword} {a.NamesSyntax} {AA}",

						Arguments = [new ("<first>", "Address of an account to get information about")],

						Examples = [new (null, $"{Keyword} {a.Name} {AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);

								var i = Rdc(new AccountRequest(First));
												
								Dump(i.Account);

								return i.Account;
							};
		return a;
	}

	public CommandAction Utility_Transfer()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "ut";

		a.Help = new() {Description = "Send utility from one account to another.",
						Syntax = $"{Keyword} {a.NamesSyntax} to={AA} by={BY}|ec={EC} {SignerArg}={AA}",

						Arguments =	[new ("to", "Account public address that funds are credited to"),
									 new ("by", "Amount of Byte-Years to be transferred"),
									 new ("ec", "Amount of Execution Cycles to be transferred"),
									 new (SignerArg, "Account public address where funds are debited from")],

						Examples =	[new (null, $"{Keyword} {a.Name} to={AA.Examples[1]} ec=1.5 {SignerArg}={AA.Examples[0]}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new UtilityTransfer(GetAccountAddress("to"), GetMoney("ec", 0), new Time(GetInt("ecexpiration", -1)), GetMoney("by", 0));
							};

		return a;
	}
}
