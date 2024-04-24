using System;
using System.Collections.Generic;
using System.IO;
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
		   		case "kp" : 
		   		case "keypair" : 
					var k = AccountKey.Create();

					Workflow.Log?.Report(this, null, [	"Public Address - " + k.ToString(), 
														"Private Key    - " + k.Key.GetPrivateKeyAsBytes().ToHex() ]);
					return null;

		   		case "c" : 
		   		case "create" : 
					return New();

				case "u" :
				case "unlock" :
				{
					Api(new UnlockWalletApc{Account = AccountAddress.Parse(Args[1].Name), 
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

			var k = AccountKey.Create();

			Workflow.Log?.Report(this, null, new string[]{	"Wallet created", 
															"Public Address - " + k.ToString(), 
															"Private Key    - " + k.Key.GetPrivateKeyAsBytes().ToHex() });

			Api(new AddWalletApc {Wallet = Program.Zone.Cryptography.Encrypt(k, p)});
			Api(new SaveWalletApc {Account = k});

			return k;
		}

		public AccountAddress Import() /// from private key
		{
			byte[] w;
			
			if(Has("privatekey"))
			{
				string p = GetString("password", null);

				if(p == null)
				{
					Program.PasswordAsker.Create(Vault.PasswordWarning);
					p = Program.PasswordAsker.Password;
				}

				var k = new AccountKey(GetBytes("privatekey"));
				w = Program.Zone.Cryptography.Encrypt(k, p);
			}
			else if(Has("wallet"))
			{
				w = File.ReadAllBytes(GetString("wallet"));
			}
			else
				throw new SyntaxException("'privatekey' or 'wallet' must be provided");

			var a = Program.Zone.Cryptography.AccountFromWallet(w);

			Api(new AddWalletApc {Wallet = w});
			Api(new SaveWalletApc {Account = a});

			Workflow.Log?.Report(this, null, new string[] {	"Wallet imported", 
															"Account Address - " + a.ToString()});
			
			return a;
		}
	}
}
