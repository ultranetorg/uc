using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System;
using Org.BouncyCastle.Utilities.Encoders;
using Uccs.Net;
using System.Collections;

namespace Uccs.Sun.CLI
{
	public abstract class Command
	{
		//public string			ProductsDirectory => Path.Join(Assembly.GetEntryAssembly().Location, "..");
		protected Settings		Settings; 
		protected Xon			Args;
		protected Net.Sun		Sun => GetCore();
		protected Func<Net.Sun>	GetCore;
		public static bool		ConsoleSupported { get; protected set; }
		public Workflow			Workflow { get; }
		public Zone				Zone;

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

		protected Command(Zone zone, Settings settings, Workflow workflow, Func<Net.Sun> getcore, Xon args)
		{
			Zone = zone;
			Settings = settings;
			GetCore = getcore;
			Args = args;
			Workflow = workflow ?? new Workflow(new Log());
		}

		protected AccountAddress GetAccountAddress(string paramenter)
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
								while(waitiftrue() && (!ConsoleSupported || !Console.KeyAvailable) && !Workflow.Cancellation.IsCancellationRequested)
								{
									Thread.Sleep(1); 
								}
							})
							.Wait();
		}

		public void Dump(object o)
		{
			void save(string name, Type type, object value, int tab)
			{
				if(type.GetInterfaces().Any(i => i == typeof(ICollection)))
				{
					Workflow.Log?.Report(new string(' ', tab * 3) + $"{name} : {{{(value as ICollection)?.Count}}}");
				
					//if(value is ICollection c)
					//	foreach(var i in c)
					//		save(i.GetType().Name, i.GetType(), i, tab + 1);
				}
				else
					Workflow.Log?.Report(new string(' ', tab * 3) + $"{name} : {value}");
			}

			foreach(var i in o.GetType().GetProperties().Where(i => i.CanRead && i.CanWrite && i.SetMethod.IsPublic))
			{
				save(i.Name, i.PropertyType, i.GetValue(o), 1);
			}
		}

		protected void Dump(XonDocument document)
		{
			document.Dump((n, l) => Workflow.Log?.Report(this, new string(' ', (l+1) * 3) + n.Name + (n.Value == null ? null : (" = "  + n.Serializator.Get<String>(n, n.Value)))));
		}

		public PlacingStage GetAwaitStage()
		{
			if(Args.Has("await"))
			{
				switch(Args.GetString("await"))
				{
					case "null" :				return PlacingStage.Null;
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
