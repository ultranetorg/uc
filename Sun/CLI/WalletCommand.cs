﻿using System;
using System.Linq;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	public class WalletCommand : Command
	{
		public const string Keyword = "wallet";

		public WalletCommand(Program program, Xon args) : base(program, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
		   		case "c" : 
		   		case "create" : 
					return New();

				case "u" :
				case "unlock" :
				{
					Api(new UnlockWalletCall {	Account = AccountAddress.Parse(Args.Nodes[1].Name), 
												Password = GetString("password")});
					return null;
				}

				case "i" : 
				case "import" : 
					return Import();
		   		
				case "e" :
				case "entity" :
				{
					var i = Rdc<AccountResponse>(new AccountRequest {Account = AccountAddress.Parse(Args.Nodes[1].Name)});
	
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

			Workflow.Log?.Report(this, null, new string[]{	"Account created", 
															"Public Address - " + acc.ToString(), 
															"Private Key    - " + acc.Key.GetPrivateKey() });

			Api(new AddWalletCall {PrivateKey = acc.Key.GetPrivateKeyAsBytes(), Password = p});
			Api(new SaveWalletCall {Account = acc});

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

			Api(new AddWalletCall {PrivateKey = acc.Key.GetPrivateKeyAsBytes(), Password = p});
			Api(new SaveWalletCall {Account = acc});

			Workflow.Log?.Report(this, null, new string[] {	"Account imported", 
															"Public Address - " + acc.ToString(), 
															"Private Key    - " + acc.Key.GetPrivateKey() });
			
			return acc;
		}
	}
}