namespace Uccs.Net;

public class AccountCommand : McvCommand
{
	public const string		Keyword = "account";

	AccountIdentifier		First => AccountIdentifier.Parse(Args[0].Name);

	public AccountCommand(McvCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		Actions =	[
						new ()
						{
							Names = ["e", "entity"],

							Help = new Help	{ 
												Title = "Entity",
												Description = "Get account entity information from Ultranet distributed database",
												Syntax = $"account e|entity {AA}",

												Arguments = [new ("<first>", "Address of an account to get information about")],

												Examples = [new (null, "account e 0x0000fffb3f90771533b1739480987cee9f08d754")]
											},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcQueryTimeout);

												var i = Rdc(new AccountRequest(First));
												
												Dump(i.Account);

												return i.Account;
											}
						},

						new ()
						{
							Names = ["ut", "utilitytransfer"],

							Help = new Help { 
												Title = "Utility Transfer",
												Description = "Send utility from one account to another.",
												Syntax = $"{Keyword} ut|utilitytransfer to={AA} by={BY}|ec={EC} signer={AA}",

												Arguments =	[new ("to", "Account public address that funds are credited to"),
															 new ("by", "Amount of Byte-Years to be transferred"),
															 new ("ec", "Amount of Execution Cycles to be transferred"),
															 new (SignerArg, "Account public address where funds are debited from")],

												Examples =	[new (null, $"{Keyword} ut to=0x1111dae119f210c94b4cf99385841fea988fcfca ec=1.5 signer=0x0000fffb3f90771533b1739480987cee9f08d754")]
											},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcTransactingTimeout);

												return new UtilityTransfer(GetAccountAddress("to"), GetMoney("ec", 0), new Time(GetInt("ecexpiration", -1)), GetMoney("by", 0));
											}
						},

					];		
	}
}
