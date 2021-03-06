using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Signer;

namespace UC.Net.Node.CLI
{
	/// <summary>
	/// Usage:	account new
	/// 
	///			account import 
	///						privatekey = PRIVATEKEY
	///						
	///			author overview
	///						address = ACCOUNT	
	/// </summary>
	public class AccountCommand : Command
	{
		public const string Keyword = "account";

		public AccountCommand(Settings settings, Log log, Func<Core> core, Xon args) : base(settings, log, core, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
		   		case "new" : return New();
				
				case "import" : return Import();
				
				case "convertfathers" :
					foreach(var i in Directory.EnumerateFiles(Settings.Secret.Fathers, "*.uwe"))
					{
						Cryptography.Current = new EthereumCryptography();
						var a = PrivateAccount.Load(i, Settings.Secret.Password);
						Cryptography.Current = new NoCryptography();
						a.Save(Path.Join(Settings.Secret.Fathers, Path.GetFileNameWithoutExtension(i)) + "." + Vault.NoCryptoWalletExtention, Settings.Secret.Password);
						Flowvizor.Log?.Report(this, null, a.ToString());
					}
					return null;
		   		
				case "overview" :
				{
					var i = Node.Connect(Role.Chain, null, Flowvizor).GetAccountInfo(Account.Parse(GetString("address")), Args.Has("confirmed"));
					
					Flowvizor.Log?.Report(this, "Account", GetString("address") + " :");

					i.Info.Dump((d, m, n, v) => Flowvizor.Log?.Report(this, null, "    " + new string(' ', d*4) + string.Format($"{{0,-{m - d*4}}}", n) + (v != null ? (" : " + v) : "")));

					return null;
				}

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}

		public PrivateAccount New()
		{
			var c = Console.ForegroundColor;
									
			string p = null;

			Console.WriteLine();
			Console.WriteLine("    ACCOUNT CREATION");
			
			if(Args.Has("password"))
			{
				p = Args.GetString("password");
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
					

			var acc = PrivateAccount.Create();

			Flowvizor.Log?.Report(this, "Account created", null, "Public Address - " + acc.ToString(), "Private Key    - " + acc.Key.GetPrivateKey());

			Core.Vault.SaveAccount(acc, p);

			return acc;
		}

		PrivateAccount Import() /// from private key
		{
			var c = Console.ForegroundColor;
									
			string p = null;
			string pc = null;

			var acc = PrivateAccount.Parse(GetString("privatekey"));

			if(Core.Vault.Accounts.Contains(acc))
			{
				Flowvizor.Log?.ReportError(this, $"Account already exists: " + acc);
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

			Flowvizor.Log?.Report(this, "Account imported", null, "Public Address - " + acc.ToString(), "Private Key    - " + acc.Key.GetPrivateKey());

			Core.Vault.SaveAccount(acc, p);

			return acc;
		}
	}
}
