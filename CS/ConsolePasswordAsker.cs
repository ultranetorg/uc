using System;
using Uccs.Net;

namespace Uccs
{
	public class ConsolePasswordAsker : IPasswordAsker
	{
		public string Password { get; protected set; }

		public bool Ask(string account)
		{
			var c = Console.ForegroundColor;
									
			string p = "";

			Console.WriteLine();
			Console.WriteLine($"A password required to access {account}");

			do 
			{
				Console.WriteLine();
	
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write("Enter password  : ");
				Console.ForegroundColor = c;

				ConsoleKey key;
				
				do
				{
					var keyInfo = Console.ReadKey(intercept: true);
					key = keyInfo.Key;

					if(key == ConsoleKey.Backspace && p.Length > 0)
					{
						Console.Write("\b \b");
						p = p[0..^1];
					}
					else if(!char.IsControl(keyInfo.KeyChar))
					{
						Console.Write("*");
						p += keyInfo.KeyChar;
					}
				}
				while(key != ConsoleKey.Enter);
				
				Console.WriteLine();

				if(string.IsNullOrWhiteSpace(p))
				{
					ShowError("Password is empty or whitespace");
					continue;
				}

				Console.WriteLine();

				Password = p;
				return true;
			}
			while(true);
		}

		public void Create(string[] warning)
		{
			var c = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.DarkGreen;

			Console.WriteLine();
			Console.WriteLine("    Password Creation");
			Console.WriteLine();
			Console.WriteLine("    Suggestions:");
			Console.WriteLine();

			foreach(var i in warning)
			{
				Console.WriteLine("    " + i + Environment.NewLine);
			}

			string pc = null;

			do 
			{
				Console.ForegroundColor = c;
	
				Console.Write("Create password  : ");
				Password = Console.ReadLine();
	
				if(string.IsNullOrWhiteSpace(Password))
				{
					Console.WriteLine();
					Console.WriteLine("Password is empty or whitespace");
					Console.WriteLine();
					continue;
				}
	
				Console.Write("Confirm password : ");
				pc = Console.ReadLine();
	
				if(pc != Password)
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

		public void ShowError(string message)
		{
			var c = Console.ForegroundColor;

			Console.ForegroundColor = ConsoleColor.Red;

			Console.WriteLine();
			Console.WriteLine(message);

			Console.ForegroundColor = c;
		}
	}
}
