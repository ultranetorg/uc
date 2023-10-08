using System.IO;
using System.Linq;
using System.Numerics;
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
	/// unt emit from{wallet=a:\Ultranet\d061eecb93844a8cabea06d80976d5958f48a343.mew} amount=0.123 to{account=0x0007f34bc43d41cf3ec2e6f684c7b9b131b04b41}
	/// unt transfer from=0x000038a7a3cb80ec769c632b7b3e43525547ecd1 to=0x00015326bcf44c84a605afbdd5343de4aaf11387 amount=5.000
	/// </example>
	public class MoneyCommand : Command
	{
		public const string Keyword = "money";

		public MoneyCommand(Zone zone, Settings settings, Workflow workflow, Net.Sun sun, Xon args) : base(zone, settings, workflow, sun, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
		   		case "emit" :
				{
					Nethereum.Web3.Accounts.Account from;

					if(Args.Has("from/key"))
					{
						from = new Nethereum.Web3.Accounts.Account(GetString("from/key"), Zone.EthereumNetwork);
					}
					else
					{
						string p;

						if(Args.Has("from/password"))
						{
							p = GetString("from/password");
						}
						else
						{
							var a = new ConsolePasswordAsker();
							a.Ask(GetString("from/wallet"));
							p = a.Password;
						}

						from = Nethereum.Web3.Accounts.Account.LoadFromKeyStore(File.ReadAllText(GetString("from/wallet")), 
																				p, 
																				new BigInteger((int)Zone.EthereumNetwork));
					}

					return Sun.Emit(from,
									Web3.Convert.ToWei(GetString("amount")),
									Sun.Vault.GetKey(GetAccountAddress("by")), 
									Command.GetAwaitStage(Args),
									Workflow);
				}

		   		case "transfer" : 
				{
					return new UntTransfer(AccountAddress.Parse(GetString("to")), Money.ParseDecimal(GetString("amount")));
				}

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
