using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Org.BouncyCastle.Utilities.Encoders;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	public abstract class Command
	{
		protected Settings		Settings; 
		protected Xon			Args;
		public Net.Sun			Sun;
		public static bool		ConsoleAvailable { get; protected set; }
		public Workflow			Workflow { get; }
		public Zone				Zone;
		public const string		AwaitArg = "await";

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

		protected Command(Zone zone, Settings settings, Workflow workflow, Net.Sun sun, Xon args)
		{
			Zone = zone;
			Settings = settings;
			Sun = sun;
			Args = args;
			Workflow = workflow;
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

		protected string GetString(string paramenter)
		{
			if(Args.Has(paramenter))
				return Args.Get<string>(paramenter);
			else
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
		}

		protected long GetLong(string paramenter)
		{
			if(Args.Has(paramenter))
				return long.Parse(Args.Get<string>(paramenter));
			else
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
		}

		protected byte[] GetHexBytes(string paramenter)
		{
			if(Args.Has(paramenter))
				return Hex.Decode(Args.Get<string>(paramenter));
			else
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
		}

		protected Version GetVersion(string paramenter)
		{
			if(Args.Has(paramenter))
				return Version.Parse(Args.Get<string>(paramenter));
			else
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
		}

		protected string GetStringOrEmpty(string paramenter)
		{
			if(Args.Has(paramenter))
				return Args.Get<string>(paramenter);
			else
				return string.Empty;
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

		protected void Wait(Func<bool> waitiftrue)
		{
			Task.Run(() =>	{
								while(waitiftrue() && (!ConsoleAvailable || !Console.KeyAvailable) && !Workflow.Cancellation.IsCancellationRequested)
								{
									Thread.Sleep(1); 
								}
							})
							.Wait();
		}

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
					else if(value is IEnumerable<string> ||
							value is IEnumerable<IPAddress>)
					{
						Workflow.Log?.Report(new string(' ', tab * 3) + $"{name} : [{string.Join(", ", value as IEnumerable<object>)}]");
					}
					else
						Workflow.Log?.Report(new string(' ', tab * 3) + $"{name} : {{{e.Count}}}");
				}
				else
					Workflow.Log?.Report(new string(' ', tab * 3) + $"{name} : {value}");
			}

			foreach(var i in o.GetType().GetProperties().Where(i => i.CanRead && i.CanWrite && i.SetMethod.IsPublic))
			{
				dump(i.Name, i.GetValue(o), 1);
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
