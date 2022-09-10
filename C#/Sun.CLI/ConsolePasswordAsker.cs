using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UC.Net.Node.CLI
{
	class ConsolePasswordAsker : IPasswordAsker
	{
		public string Password { get; protected set; }

		public bool Ask(string account)
		{
			var c = Console.ForegroundColor;
									
			string p = "";

			Console.WriteLine();
			Console.WriteLine($"A password required to access {account} account");

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

					if (key == ConsoleKey.Backspace && p.Length > 0)
					{
						Console.Write("\b \b");
						p = p[0..^1];
					}
					else if (!char.IsControl(keyInfo.KeyChar))
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
