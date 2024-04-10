using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	/// <summary>
	/// Usage: unt emit from {
	///							wallet = PATH 
	///							[password = PASSWORD] 
	///						 }
	///					amount = UNT
	///					to	 {
	///							account = ACCOUNT 
	///							[password = PASSWORD]
	///						 }
	///					[awaitconfirmation]
	///
	/// Usage: unt transfer from = ACCOUNT 
	///						[password = PASSWORD] 
	///						to = ACCOUNT 
	///						amount = UNT 
	/// </summary>
	///	<example>
	/// 
	/// money emit from{wallet=m:\UO\Team\Maximion\0x321D3AB8998c551Aeb086a7AC28635261AC66c00.mew} amount=1 by=0x001fea628d33830e5515e52fb7e3f9a009b24317
	/// 
	/// </example>
	public class MoneyCommand : Command
	{
		public const string Keyword = "money";

		public MoneyCommand(Program program, List<Xon> args) : base(program, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.First().Name)
			{
		   		case "e" :
		   		case "emit" :
				{
					Workflow.CancelAfter(RdcTransactingTimeout);

					Nethereum.Web3.Accounts.Account from;

					if(Has("from/key"))
					{
						from = new Nethereum.Web3.Accounts.Account(GetString("from/key"), Program.Zone.EthereumNetwork);
					}
					else
					{
						string p;

						if(Has("from/password"))
						{
							p = GetString("from/password");
						}
						else
						{
							Program.PasswordAsker.Ask(GetString("from/wallet"));
							p = Program.PasswordAsker.Password;
						}

						from = Nethereum.Web3.Accounts.Account.LoadFromKeyStore(File.ReadAllText(GetString("from/wallet")), 
																				p, 
																				new BigInteger((int)Program.Zone.EthereumNetwork));
					}

					Api<Transaction>(new EmitApc{	FromPrivateKey = from.PrivateKey.HexToByteArray(),
													Wei = Web3.Convert.ToWei(GetString("amount")),
													To = GetAccountAddress("by"), 
													Await = GetAwaitStage(Args) });
					return null;
				}

		   		case "t" : 
		   		case "transfer" : 
				{
					Workflow.CancelAfter(RdcTransactingTimeout);

					return new UntTransfer(AccountAddress.Parse(GetString("to")), Money.ParseDecimal(GetString("amount")));
				}

		   		case "c" : 
		   		case "cost" : 
				{
					Workflow.CancelAfter(RdcTransactingTimeout);

					var r = Api<CostApc.Report>(new CostApc {Rate = GetMoney("rate", Money.Zero)});
					
					Dump(r);

					return r;
				}

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
