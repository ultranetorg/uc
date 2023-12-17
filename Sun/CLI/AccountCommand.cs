using System;
using System.Linq;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	public class AccountCommand : Command
	{
		public const string Keyword = "account";

		public AccountCommand(Program program, Xon args) : base(program, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
		   		case "n" : 
		   		case "new" : 
					return New();

				case "u" :
				case "unlock" :
				{
					Program.Api(new UnlockWalletCall {	Account = AccountAddress.Parse(Args.Nodes[1].Name), 
														Password = GetString("password")});
					//Sun.Vault.Unlock(AccountAddress.Parse(Args.Nodes[1].Name), GetString("password"));
					return null;
				}

				case "i" : 
				case "import" : 
					return Import();
		   		
				case "e" :
				case "entity" :
				{
					var i = Program.Rdc<AccountResponse>(new AccountRequest {Account = AccountAddress.Parse(Args.Nodes[1].Name)});

					//var i = Sun.Call(i => i.GetAccountInfo(AccountAddress.Parse(Args.Nodes[1].Name)), Workflow);
	
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
				Program.PasswordAsker.Create();
				p = Program.PasswordAsker.Password;
			}

			var acc = AccountKey.Create();

			Workflow.Log?.Report(this, "Account created", null, "Public Address - " + acc.ToString(), "Private Key    - " + acc.Key.GetPrivateKey());

			Program.Api(new AddWalletCall {PrivateKey = acc.Key.GetPrivateKeyAsBytes(), Password = p});
			Program.Api(new SaveWalletCall {Account = acc});

			return acc;
		}

		AccountKey Import() /// from private key
		{
			AccountKey acc;
			
			if(Args.Has("privatekey"))
				acc = AccountKey.Parse(GetString("privatekey"));
			else if(Args.Has("wallet"))
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
				Program.PasswordAsker.Create();
				p = Program.PasswordAsker.Password;
			}

			Program.Api(new AddWalletCall {PrivateKey = acc.Key.GetPrivateKeyAsBytes(), Password = p});
			Program.Api(new SaveWalletCall {Account = acc});

			Workflow.Log?.Report(this, "Account imported", null, "Public Address - " + acc.ToString(), "Private Key    - " + acc.Key.GetPrivateKey());
			
			return acc;
		}
	}
}
