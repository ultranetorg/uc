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
		protected Dispatcher	Dispatcher => DispatcherFunc();
		protected Func<Dispatcher>	DispatcherFunc;
		protected static bool	ConsoleSupported = true;
		Operation				Operation;

		Log				IFlowControl.Log => Log;

		public abstract object Execute();

		protected Dispatcher Client
		{
			get
			{
				if(Dispatcher.IsClient)
					return Dispatcher;

				Dispatcher.RunClient(null, () => ConsoleSupported && Console.KeyAvailable);

				return Dispatcher;
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

		protected Command(Settings settings, Log log, Func<Dispatcher> dispatcher, Xon args)
		{
			Settings = settings;
			Log = log;
			DispatcherFunc = dispatcher;
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
				if(Args.Has("await") && Args.GetString("await") == "placing")
					Wait(() => o.Stage != ProcessingStage.Placed && o.Stage != ProcessingStage.Delegated);
				else /// confirmation
					Wait(() => o.Transaction == null || o.Transaction.Stage != ProcessingStage.Confirmed);
			}


			return obj;
		}
	}
}
