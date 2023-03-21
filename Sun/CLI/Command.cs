using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System;
using Org.BouncyCastle.Utilities.Encoders;
using UC.Net;

namespace UC.Sun.CLI
{
	public abstract class Command
	{
		//public string			ProductsDirectory => Path.Join(Assembly.GetEntryAssembly().Location, "..");
		protected Settings		Settings; 
		protected Xon			Args;
		protected Core			Core => GetCore();
		protected Func<Core>	GetCore;
		public static bool		ConsoleSupported { get; protected set; }
		public Workflow			Workflow { get; }

		public abstract object Execute();

		static Command()
		{
			try
			{
				var p = Console.KeyAvailable;
				ConsoleSupported = true;
			}
			catch(Exception)
			{
				ConsoleSupported = false;
			}
		}

		protected Command(Settings settings, Log log, Func<Core> getcore, Xon args)
		{
			Settings = settings;
			GetCore = getcore;
			Args = args;
			Workflow = new Workflow(log);
		}

		protected string GetString(string paramenter)
		{
			if(Args.Has(paramenter))
				return Args.GetString(paramenter);
			else
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
		}

		protected long GetLong(string paramenter)
		{
			if(Args.Has(paramenter))
				return long.Parse(Args.GetString(paramenter));
			else
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
		}

		protected byte[] GetHexBytes(string paramenter)
		{
			if(Args.Has(paramenter))
				return Hex.Decode(Args.GetString(paramenter));
			else
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
		}

		protected Version GetVersion(string paramenter)
		{
			if(Args.Has(paramenter))
				return Version.Parse(Args.GetString(paramenter));
			else
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
		}

		protected string GetStringOrEmpty(string paramenter)
		{
			if(Args.Has(paramenter))
				return Args.GetString(paramenter);
			else
				return string.Empty;
		}

		protected AccountKey GetPrivate(string walletarg, string passwordarg)
		{
			string p = null;
			
			if(Args.Has(passwordarg))
				p = GetString(passwordarg);
			else if(Settings.Secret != null)	
				p = Settings.Secret.Password;
			else
			{
				var a = new ConsolePasswordAsker();
				a.Ask(GetString(walletarg));
				p = a.Password; 
			}

			return Core.Vault.Unlock(Account.Parse(GetString(walletarg)), p);
		}

		protected void Wait(Func<bool> waitiftrue)
		{
			Task.Run(() =>	{
								while(waitiftrue() && (!ConsoleSupported || !Console.KeyAvailable) && !Workflow.Cancellation.IsCancellationRequested)
								{
									Thread.Sleep(1); 
								}
							})
							.Wait();
		}

		//protected object Send(Func<object> create)
		//{
		//	var obj = create();
		//
		//	if(obj is Operation o)
		//	{
		//		if(Args.Has("await"))
		//			Wait(() =>	{
		//							switch(Args.GetString("await"))
		//							{
		//								case "accepted" :			return o.Placing < PlacingStage.Accepted;
		//								case "placed" :				return o.Placing < PlacingStage.Placed;
		//								case "confirmed" :			return o.Placing != PlacingStage.Confirmed;
		//								case "failedornotfound" :	return o.Placing != PlacingStage.FailedOrNotFound;
		//							}
		//											
		//							throw new SyntaxException("Unknown awaiting stage");
		//						});
		//		else
		//			Wait(() => o.Placing < PlacingStage.Placed);
		//	}
		//
		//	return obj;
		//}

		protected void Dump(XonDocument document)
		{
			document.Dump((n, l) => Workflow.Log?.Report(this, null, new string(' ', (l+1) * 3) + n.Name + (n.Value == null ? null : (" = "  + n.Serializator.Get<String>(n, n.Value)))));
		}

		public PlacingStage GetAwaitStage()
		{
			if(Args.Has("await"))
			{
				switch(Args.GetString("await"))
				{
					case "accepted" :			return PlacingStage.Accepted;
					case "placed" :				return PlacingStage.Placed;
					case "confirmed" :			return PlacingStage.Confirmed;
					case "failedornotfound" :	return PlacingStage.FailedOrNotFound;
				}
			
				throw new SyntaxException("Unknown awaiting stage");
			}
			else
				return PlacingStage.Placed;
		}
	}
}
