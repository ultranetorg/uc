using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Signer;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	public class AccountCommand : Command
	{
		public const string Keyword = "account";

		public AccountCommand(Zone zone, Settings settings, Workflow workflow, Net.Sun sun, Xon args) : base(zone, settings, workflow, sun, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
		   		case "new" : return New();

				case "unlock" :
				{
					Sun.Vault.Unlock(GetAccountAddress("address"), GetString("password"));
					return null;
				}

				case "import" : return Import();
		   		
				case "info" :
				{
					try
					{
						var i = Sun.Call(i => i.GetAccountInfo(GetAccountAddress("address")), Workflow);
	
						Dump(i.Account);

						return i.Account;
					}
					catch(RdcEntityException ex)
					{
						Workflow.Log?.Report(this, ex.Message);
					}

					return null;
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

			Sun.Vault.AddWallet(acc, p);
			Sun.Vault.SaveWallet(acc);

			return acc;
		}

		AccountKey Import() /// from private key
		{
			var c = Console.ForegroundColor;
									
			string p = null;
			string pc = null;

			var acc = AccountKey.Parse(GetString("privatekey"));

			if(Sun.Vault.Wallets.ContainsKey(acc))
			{
				Workflow.Log?.ReportError(this, $"Account already exists: " + acc);
				return null;
			}

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

			Sun.Vault.AddWallet(acc, p);
			Sun.Vault.SaveWallet(acc);

			Workflow.Log?.Report(this, "Account imported", null, "Public Address - " + acc.ToString(), "Private Key    - " + acc.Key.GetPrivateKey());
			
			return acc;
		}
	}
}
