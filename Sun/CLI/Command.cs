using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	public abstract class Command
	{
		protected Program		Program;
		protected Xon			Args;
		public static bool		ConsoleAvailable { get; protected set; }
		public const string		AwaitArg = "await";

		public Workflow			Workflow;
		protected int			RdcQueryTimeout = 5000;
		protected int			RdcTransactingTimeout = 60*1000;

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

		public void Api(SunApiCall call)
		{
			if(Program.ApiClient == null)
				call.Execute(Program.Sun, Workflow);
			else
				Program.ApiClient.Send(call, Workflow);
		}

		public Rp Api<Rp>(SunApiCall call)
		{
			if(Program.ApiClient == null) 
				return (Rp)call.Execute(Program.Sun, Workflow);
			else
				return Program.ApiClient.Request<Rp>(call, Workflow);
		}

		public Rp Rdc<Rp>(RdcRequest request) where Rp : RdcResponse
		{
			if(Program.ApiClient == null) 
			{
				return Program.Sun.Call(i => i.Request(request), Workflow) as Rp;
			}
			else
			{
				var rp = Api<Rp>(new RdcCall {Request = request});
 
 				if(rp.Error != null)
 					throw rp.Error;
 
				return rp;
			}
		}

		public void Enqueue(IEnumerable<Operation> operations, AccountAddress by, PlacingStage await)
		{
			if(Program.ApiClient == null)
				Program.Sun.Enqueue(operations, by, await, Workflow);
			else
				Program.ApiClient.Send(new EnqeueOperationCall {Operations = operations,
																By = by,
																Await = await},
										Workflow);
		}

		public bool Has(string paramenter)
		{
			return Args.Has(paramenter);
		}

		public AccountAddress GetAccountAddress(string paramenter, bool mandatory = true)
		{
			if(Args.Has(paramenter))
				return AccountAddress.Parse(GetString(paramenter));
			else
				if(mandatory)
					throw new SyntaxException($"Parameter '{paramenter}' not provided");
				else
					return null;
		}

		protected ResourceAddress GetResourceAddress(string paramenter, bool mandatory = true)
		{
			if(Args.Has(paramenter))
				return ResourceAddress.Parse(GetString(paramenter));
			else
				if(mandatory)
					throw new SyntaxException($"Parameter '{paramenter}' not provided");
				else
					return null;
		}

		protected PackageAddress GetReleaseAddress(string paramenter, bool mandatory = true)
		{
			if(Args.Has(paramenter))
				return PackageAddress.Parse(GetString(paramenter));
			else
				if(mandatory)
					throw new SyntaxException($"Parameter '{paramenter}' not provided");
				else
					return null;
		}

		protected string GetString(string paramenter, bool mandatory = true)
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

		protected byte[] GetHexBytes(string paramenter, bool mandatory = true)
		{
			if(Args.Has(paramenter))
				return Args.Get<string>(paramenter).FromHex();
			else
				if(mandatory)
					throw new SyntaxException($"Parameter '{paramenter}' not provided");
				else
					return null;
		}

		protected Version GetVersion(string paramenter, bool mandatory = true)
		{
			if(Args.Has(paramenter))
				return Version.Parse(Args.Get<string>(paramenter));
			else
				if(mandatory)
					throw new SyntaxException($"Parameter '{paramenter}' not provided");
				else
					return null;
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

		protected E GetEnum<E>(string paramenter, E def) where E : struct
		{
			if(Args.Has(paramenter))
				return Enum.Parse<E>(Args.Get<string>(paramenter));
			else
				return def;
		}

		protected E GetEnum<E>(string paramenter) where E : struct
		{
			if(Args.Has(paramenter))
				return Enum.Parse<E>(Args.Get<string>(paramenter));
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
