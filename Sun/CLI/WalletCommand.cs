using System;
using System.Collections.Generic;
using System.Linq;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	public class WalletCommand : Command
	{
		public const string Keyword = "wallet";

		public WalletCommand(Program program, List<Xon> args) : base(program, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.First().Name)
			{
		   		case "c" : 
		   		case "create" : 
					return New();

				case "u" :
				case "unlock" :
				{
					Api(new UnlockWalletApc {	Account = AccountAddress.Parse(Args[1].Name), 
												Password = GetString("password")});
					return null;
				}

				case "i" : 
				case "import" : 
					return Import();
		   		
				case "e" :
				case "entity" :
				{
					var i = Rdc(new AccountRequest {Account = AccountAddress.Parse(Args[1].Name)});
	
					Dump(i.Account);

					return i.Account;
				}

				default:
					throw new SyntaxException("Unknown operation");
			}
		}

		public AccountKey New()
		{
			string p = GetString("password", null);

			if(p == null)
			{
				Program.PasswordAsker.Create(Vault.PasswordWarning);
				p = Program.PasswordAsker.Password;
			}

			var acc = AccountKey.Create();

			Workflow.Log?.Report(this, null, new string[]{	"Wallet created", 
															"Public Address - " + acc.ToString(), 
															"Private Key    - " + acc.Key.GetPrivateKey() });

			Api(new AddWalletApc {PrivateKey = acc.Key.GetPrivateKeyAsBytes(), Password = p});
			Api(new SaveWalletApc {Account = acc});

			return acc;
		}

		AccountKey Import() /// from private key
		{
			AccountKey acc;
			
			if(Has("privatekey"))
				acc = AccountKey.Parse(GetString("privatekey"));
			else if(Has("wallet"))
			{
				var wp = GetString("wallet/password", null);

				if(wp == null)
				{
					Program.PasswordAsker.Ask(GetString("wallet/path"));
					wp = Program.PasswordAsker.Password;
				}

				acc = AccountKey.Load(Program.Zone.Cryptography, GetString("wallet/path"), wp);
			}
			else
				throw new SyntaxException("'privatekey' or 'wallet' must be provided");

			string p = GetString("password", null);

			if(p == null)
			{
				Program.PasswordAsker.Create(Vault.PasswordWarning);
				p = Program.PasswordAsker.Password;
			}

			Api(new AddWalletApc {PrivateKey = acc.Key.GetPrivateKeyAsBytes(), Password = p});
			Api(new SaveWalletApc {Account = acc});

			Workflow.Log?.Report(this, null, new string[] {	"Wallet imported", 
															"Public Address - " + acc.ToString(), 
															"Private Key    - " + acc.Key.GetPrivateKey() });
			
			return acc;
		}
	}
}
