using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System;
using Org.BouncyCastle.Utilities.Encoders;

namespace UC.Net.Node.CLI
{
	public abstract class Command
	{
		protected Settings					Settings; 
		protected Xon						Args;
		protected Core						Core => CoreFunc();
		protected Func<Core>				CoreFunc;
		public static bool					ConsoleSupported { get; protected set; }
		//Operation							Operation;
		public Flowvizor					Flowvizor {get;}

		public abstract object Execute();

		protected Core Node
		{
			get
			{
				if(Core.IsNodee)
					return Core;

				Core.RunNode();

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
			CoreFunc = core;
			Args = args;
			Flowvizor = new Flowvizor(log);
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

		protected void Wait(Func<bool> waitiftrue)
		{
			Task.Run(() =>	{
								while(waitiftrue() && (!ConsoleSupported || !Console.KeyAvailable) && !Flowvizor.Cancellation.IsCancellationRequested) 
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

			return Node.Vault.Unlock(Account.Parse(GetString(walletarg)), p);
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
					Wait(() =>	o.Delegation != DelegationStage.Completed);
			}

			return obj;
		}

		protected void Dump(XonDocument document)
		{
			document.Dump((n, l) => Flowvizor.Log?.Report(this, null, new string(' ', (l+1) * 3) + n.Name + (n.Value == null ? null : (" = "  + n.Serializator.Get<String>(n, n.Value)))));
		}

	}
}
