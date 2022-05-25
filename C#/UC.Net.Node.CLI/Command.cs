using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System;

namespace UC.Net.Node.CLI
{
	public abstract class Command : IFlowControl
	{
		protected Settings		Settings; 
		protected Log			Log; 
		protected Xon			Args;
		protected Core			Core => CoreFunc();
		protected Func<Core>	CoreFunc;
		protected static bool	ConsoleSupported = true;
		Operation				Operation;

		Log				IFlowControl.Log => Log;

		public abstract object Execute();

		protected Core Client
		{
			get
			{
				if(Core.IsClient)
					return Core;

				Core.RunClient(null, () => ConsoleSupported && Console.KeyAvailable);

				return Core;
			}
		}

		static Command()
		{
			try
			{
				var p = Console.KeyAvailable;
			}
			catch(Exception)
			{
				ConsoleSupported = false;
			}
		}

		protected Command(Settings settings, Log log, Func<Core> core, Xon args)
		{
			Settings = settings;
			Log = log;
			CoreFunc = core;
			Args = args;
		}

		public void	SetOperation(Operation o)
		{
			Operation = o;
		}

		public void	StageChanged()
		{
		}

		protected string GetString(string paramenter)
		{
			if(Args.Has(paramenter))
				return Args.GetString(paramenter);
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

		protected void Wait(Func<bool> waitiftrue)
		{
			Task.Run(() =>	{
								while(waitiftrue() && (!ConsoleSupported || !Console.KeyAvailable)) 
								{
									Thread.Sleep(100); 
								}
							})
							.Wait();
		}

		protected PrivateAccount GetPrivate(string walletarg, string passwordarg)
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

			return Client.Vault.Unlock(Account.Parse(GetString(walletarg)), p);
		}

		protected object Send(Func<object> create)
		{
			var obj = create();

			if(obj is Operation o)
			{
				if(Args.Has("await"))
					Wait(() =>	{
									switch(Args.GetString("await"))
									{
										case "accepted" :			return o.Placing < PlacingStage.Accepted;
										case "placed" :				return o.Placing < PlacingStage.Placed;
										case "confirmed" :			return o.Placing != PlacingStage.Confirmed;
										case "failedornotfound" :	return o.Placing != PlacingStage.FailedOrNotFound;
									}
													
									throw new SyntaxException("Unknown awaiting stage");
								});
				else
					Wait(() =>	o.Placing != PlacingStage.Confirmed);
			}

			return obj;
		}

		protected void Dump(XonDocument document)
		{
			document.Dump((n, l) => Log?.Report(this, null, new string(' ', (l+1) * 3) + n.Name + (n.Value == null ? null : (" = "  + n.Value.ToString()))));
		}

	}
}
