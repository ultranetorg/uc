﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Signer;
using Uccs.Net;

namespace Uccs.Sun.CLI
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

		public AccountCommand(Zone zone, Settings settings, Log log, Func<Core> core, Xon args) : base(zone, settings, log, core, args)
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
		   		
				case "overview" :
				{
					var i = Core.Connect(Role.Base, null, Workflow).GetAccountInfo(AccountAddress.Parse(GetString("address")));
					
					Workflow?.Log?.Report(this, "Account", GetString("address") + " :");

					Dump(i.Account.ToXon());

					return null;
				}

				default:
					throw new SyntaxException("Unknown operation");;
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
					

			var acc = AccountKey.Create();

			Workflow.Log?.Report(this, "Account created", null, "Public Address - " + acc.ToString(), "Private Key    - " + acc.Key.GetPrivateKey());

			Core.Vault.AddWallet(acc, p);

			return acc;
		}

		AccountKey Import() /// from private key
		{
			var c = Console.ForegroundColor;
									
			string p = null;
			string pc = null;

			var acc = AccountKey.Parse(GetString("privatekey"));

			if(Core.Vault.Accounts.Contains(acc))
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

			Workflow.Log?.Report(this, "Account imported", null, "Public Address - " + acc.ToString(), "Private Key    - " + acc.Key.GetPrivateKey());

			Core.Vault.AddWallet(acc, p);

			return acc;
		}
	}
}