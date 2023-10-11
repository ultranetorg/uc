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
		   		case "new" : 
					return New();

				case "unlock" :
				{
					Program.Call(new UnlockWalletCall {	Account = AccountAddress.Parse(Args.Nodes[1].Name), 
														Password = GetString("password")});
					//Sun.Vault.Unlock(AccountAddress.Parse(Args.Nodes[1].Name), GetString("password"));
					return null;
				}

				case "import" : 
					return Import();
		   		
				case "info" :
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
			var c = Console.ForegroundColor;
									
			string p = null;

			Console.WriteLine();
			Console.WriteLine("    ACCOUNT CREATION");
			
			if(Args.Has("password"))
			{
				p = Args.Get<string>("password");
			} 
			else
			{
				//Console.ForegroundColor = ConsoleColor.DarkGreen;

				string pc = null;

				Console.WriteLine();
				Console.WriteLine("    Password Suggestions:");
				Console.WriteLine();
	
				foreach(var i in Vault.PasswordWarning)
				{
					Console.WriteLine("    " + i + Environment.NewLine);
				}
	
				do 
				{
					Console.ForegroundColor = c;
	
					Console.Write("Create password  : ");
					p = Console.ReadLine();
	
					if(string.IsNullOrWhiteSpace(p))
					{
						Console.WriteLine();
						Console.WriteLine("Password is empty or whitespace");
						Console.WriteLine();
						continue;
					}
	
					Console.Write("Confirm password : ");
					pc = Console.ReadLine();
	
					if(pc != p)
					{
						Console.WriteLine();
						Console.WriteLine("Password mismatch");
						Console.WriteLine();
						continue;
					}
	
					break;
				}
				while(true);
	
				Console.WriteLine();
				Console.ForegroundColor = c;	
			}
					

			var acc = AccountKey.Create();

			Workflow.Log?.Report(this, "Account created", null, "Public Address - " + acc.ToString(), "Private Key    - " + acc.Key.GetPrivateKey());

			Program.Call(new AddWalletCall {PrivateKey = acc.Key.GetPrivateKeyAsBytes(), Password = p});
			Program.Call(new SaveWalletCall {Account = acc});

			return acc;
		}

		AccountKey Import() /// from private key
		{
			var c = Console.ForegroundColor;
									
			string p = null;
			string pc = null;

			var acc = AccountKey.Parse(GetString("privatekey"));

			Console.ForegroundColor = ConsoleColor.DarkGreen;

			Console.WriteLine();
			Console.WriteLine("    ACCOUNT IMPORT");
			Console.WriteLine();
			Console.WriteLine("    Password Suggestions:");
			Console.WriteLine();

			foreach(var i in Vault.PasswordWarning)
			{
				Console.WriteLine("    " + i + Environment.NewLine);
			}

			do 
			{
				Console.ForegroundColor = c;

				Console.Write("Create password  : ");
				p = Console.ReadLine();

				if(string.IsNullOrWhiteSpace(p))
				{
					Console.WriteLine();
					Console.WriteLine("Password is empty or whitespace");
					Console.WriteLine();
					continue;
				}

				Console.Write("Confirm password : ");
				pc = Console.ReadLine();

				if(pc != p)
				{
					Console.WriteLine();
					Console.WriteLine("Password mismatch");
					Console.WriteLine();
					continue;
				}

				break;
			}
			while(true);

			Console.WriteLine();
			Console.ForegroundColor = c;							

			Program.Call(new AddWalletCall {PrivateKey = acc.Key.GetPrivateKeyAsBytes(), Password = p});
			Program.Call(new SaveWalletCall {Account = acc});

			Workflow.Log?.Report(this, "Account imported", null, "Public Address - " + acc.ToString(), "Private Key    - " + acc.Key.GetPrivateKey());
			
			return acc;
		}
	}
}
