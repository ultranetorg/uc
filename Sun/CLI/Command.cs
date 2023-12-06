using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using Org.BouncyCastle.Utilities.Encoders;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	public abstract class Command
	{
		protected Program		Program;
		protected Xon			Args;
		//public Net.Sun			Sun;
		//protected JsonApiClient	Api;
		public static bool		ConsoleAvailable { get; protected set; }
		public const string		AwaitArg = "await";

		protected Workflow		Workflow => Program.Workflow;

		public abstract object Execute();

		static Command()
		{
			try
			{
				var p = Console.KeyAvailable;
				ConsoleAvailable = true;
			}
			catch(Exception)
			{
				ConsoleAvailable = false;
			}
		}

		protected Command(Program program, Xon args)
		{
			Program = program;
			Args = args;
		}

		public AccountAddress GetAccountAddress(string paramenter)
		{
			if(Args.Has(paramenter))
				return AccountAddress.Parse(GetString(paramenter));
			else
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
		}

		protected ResourceAddress GetResourceAddress(string paramenter)
		{
			if(Args.Has(paramenter))
				return ResourceAddress.Parse(GetString(paramenter));
			else
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
		}

		protected PackageAddress GetReleaseAddress(string paramenter)
		{
			if(Args.Has(paramenter))
				return PackageAddress.Parse(GetString(paramenter));
			else
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
		}

		protected string GetString(string paramenter, bool mandatory = false)
		{
			if(Args.Has(paramenter))
				return Args.Get<string>(paramenter);
			else
				if(mandatory)
					throw new SyntaxException($"Parameter '{paramenter}' not provided");
				else
					return null;
		}

		protected long GetLong(string paramenter)
		{
			if(Args.Has(paramenter))
				return long.Parse(Args.Get<string>(paramenter));
			else
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
		}

		protected byte[] GetHexBytes(string paramenter, bool mandatory = false)
		{
			if(Args.Has(paramenter))
				return Hex.Decode(Args.Get<string>(paramenter));
			else
				if(mandatory)
					throw new SyntaxException($"Parameter '{paramenter}' not provided");
				else
					return null;
		}

		protected Version GetVersion(string paramenter)
		{
			if(Args.Has(paramenter))
				return Version.Parse(Args.Get<string>(paramenter));
			else
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
		}

		protected string GetString(string paramenter, string def)
		{
			if(Args.Has(paramenter))
				return Args.Get<string>(paramenter);
			else
				return def;
		}

		protected Money GetMoney(string paramenter)
		{
			if(Args.Has(paramenter))
				return Money.ParseDecimal(Args.Get<string>(paramenter));
			else
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
		}

		//protected AccountKey GetPrivate(string walletarg)
		//{
		//	string p = null;
		//	
		//	var a = new ConsolePasswordAsker();
		//	a.Ask(GetString(walletarg));
		//	p = a.Password; 
		//
		//	return Sun.Vault.Unlock(AccountAddress.Parse(GetString(walletarg)), p);
		//}

		public void Dump(object o)
		{
			void dump(string name, object value, int tab)
			{
				if(value is null)
				{
					Workflow.Log?.Report(new string(' ', tab * 3) + name);
				}
				else if(value is ICollection e)
				{
					if(value is int[])
					{
						Workflow.Log?.Report(new string(' ', tab * 3) + $"{name} : [{string.Join(", ", value as int[])}]");
					}
					else if(value is byte[])
					{
						Workflow.Log?.Report(new string(' ', tab * 3) + $"{name} : {(value as byte[]).ToHex()}");
					}
					else if(value is IEnumerable<string> ||
							value is IEnumerable<IPAddress>)
					{
						Workflow.Log?.Report(new string(' ', tab * 3) + $"{name} : [{string.Join(", ", value as IEnumerable<object>)}]");
					}
					else if(value is IEnumerable<Dependency> ||
							value is IEnumerable<AnalyzerResult>)
					{
						Workflow.Log?.Report(new string(' ', tab * 3) + $"{name} :");

						foreach(var i in value as IEnumerable)
						{
							dump(null, i, tab+1);
						}
					}
					else
						Workflow.Log?.Report(new string(' ', tab * 3) + $"{name} : {{{e.Count}}}");
				}
				else if(value is Resource || 
						value is Manifest)
				{
					Workflow.Log?.Report(new string(' ', tab * 3) + $"{name}");

					foreach(var i in value.GetType().GetProperties().Where(i => i.CanRead && i.CanWrite && i.SetMethod.IsPublic))
					{
						dump(i.Name, i.GetValue(value), tab + 1);
					}
				}
				else
					Workflow.Log?.Report(new string(' ', tab * 3) + $"{(name == null ? null : (name + " : " ))}{value}");
			}

			foreach(var i in o.GetType().GetProperties().Where(i => i.CanRead && i.CanWrite && i.SetMethod.IsPublic))
			{
				dump(i.Name, i.GetValue(o), 1);
			}
		}

		public void Dump<T>(IEnumerable<T> items, string[] columns, IEnumerable<Func<T, string>> gets)
		{
			if(!items.Any())
			{	
				Workflow.Log?.Report("   No results");
				return;
			}


			string[,] t = new string[items.Count(), columns.Length];
			int[] w = columns.Select(i => i.Length).ToArray();

			var ii = 0;

			foreach(var i in items)
			{
				var gi = 0;

				foreach(var g in gets)
				{
					t[ii, gi] = g(i);
					w[gi] = Math.Max(w[gi], t[ii, gi].Length);

					gi++;
				}

				ii++;
			}

			var f = string.Join(" ", columns.Select((c, i) => $"{{{i},-{w[i]}}}"));

			Workflow.Log?.Report("   " + string.Format(f, columns));
			Workflow.Log?.Report("   " + string.Format(f, w.Select(i => new string('-', i)).ToArray()));
						
			f = string.Join(" ", columns.Select((c, i) => $"{{{i},{w[i]}}}"));

			for(int i=0; i < items.Count(); i++)
			{
				Workflow.Log?.Report("   " + string.Format(f, Enumerable.Range(0, columns.Length).Select(j => t[i, j]).ToArray()));
			}
		}

		protected void Dump(XonDocument document)
		{
			document.Dump((n, l) => Workflow.Log?.Report(this, new string(' ', (l+1) * 3) + n.Name + (n.Value == null ? null : (" = "  + n.Serializator.Get<String>(n, n.Value)))));
		}

		public static PlacingStage GetAwaitStage(Xon args)
		{
			if(args.Has(AwaitArg))
			{
				return Enum.GetValues<PlacingStage>().First(i => i.ToString().ToLower() == args.Get<string>(AwaitArg));
			}
			else
				return PlacingStage.Placed;
		}
	}
}
