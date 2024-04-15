using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	public abstract class Command
	{
		protected Program			Program;
		protected List<Xon>			Args;
		public static bool			ConsoleAvailable { get; protected set; }
		public const string			AwaitArg = "await";

		public Workflow				Workflow;
		protected int				RdcQueryTimeout = 5000;
		protected int				RdcTransactingTimeout = 60*1000;

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

		protected Command(Program program, List<Xon> args)
		{
			Program = program;
			Args = args;
		}

		public void Api(SunApc call)
		{
			if(Program.ApiClient == null)
				call.Execute(Program.Sun, null, null, Workflow);
			else
				Program.ApiClient.Send(call, Workflow);
		}

		public Rp Api<Rp>(SunApc call)
		{
			if(Program.ApiClient == null) 
				return (Rp)call.Execute(Program.Sun, null, null, Workflow);
			else
				return Program.ApiClient.Request<Rp>(call, Workflow);
		}

		public Rp Rdc<Rp>(RdcCall<Rp> request) where Rp : RdcResponse
		{
			if(Program.ApiClient == null) 
			{
				return Program.Sun.Call(i => i.Request(request), Workflow) as Rp;
			}
			else
			{
				var rp = Api<Rp>(new RdcApc {Request = request});
 
 				if(rp.Error != null)
 					throw rp.Error;
 
				return rp;
			}
		}

		public void Transact(IEnumerable<Operation> operations, AccountAddress by, TransactionStatus await)
		{
			if(Program.ApiClient == null)
				Program.Sun.Transact(operations, by, await, Workflow);
			else
				Program.ApiClient.Send(new EnqeueOperationApc {	Operations = operations,
																By = by,
																Await = await},
										Workflow);
		}

		public Xon One(string path)
		{
			var names = path.Split('/');

			var i = names.GetEnumerator();
			
			i.MoveNext();

			var p = Args.FirstOrDefault(j => j.Name.Equals(i.Current));

			if(p != null)
			{
				while(i.MoveNext())
				{
					p = p.Nodes.FirstOrDefault(j => j.Name.Equals(i.Current));

					if(p == null)
					{
						return null;
					}
				}
			}
			return p;	 
		}

		public bool Has(string paramenter)
		{
			return One(paramenter) != null;
		}

		public AccountAddress GetAccountAddress(string paramenter, bool mandatory = true)
		{
			if(Has(paramenter))
				return AccountAddress.Parse(GetString(paramenter));
			else
				if(mandatory)
					throw new SyntaxException($"Parameter '{paramenter}' not provided");
				else
					return null;
		}

		protected ResourceAddress GetResourceAddress(string paramenter, bool mandatory = true)
		{
			if(Has(paramenter))
				return ResourceAddress.Parse(GetString(paramenter));
			else
				if(mandatory)
					throw new SyntaxException($"Parameter '{paramenter}' not provided");
				else
					return null;
		}

		protected ReleaseAddress GetReleaseAddress(string paramenter, bool mandatory = true)
		{
			if(Has(paramenter))
				return ReleaseAddress.Parse(GetString(paramenter));
			else
				if(mandatory)
					throw new SyntaxException($"Parameter '{paramenter}' not provided");
				else
					return null;
		}

		protected string GetString(string paramenter, bool mandatory = true)
		{
			var p = One(paramenter);

			if(p != null)
				return p.Get<string>();
			else
				if(mandatory)
					throw new SyntaxException($"Parameter '{paramenter}' not provided");
				else
					return null;
		}

		protected long GetLong(string paramenter)
		{
			var p = One(paramenter);

			if(p != null)
				return long.Parse(p.Get<string>());
			else
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
		}

		protected byte[] GetBytes(string paramenter, bool mandatory = true)
		{
			var p = One(paramenter);

			if(p != null)
				return p.Get<string>().FromHex();
			else
				if(mandatory)
					throw new SyntaxException($"Parameter '{paramenter}' not provided");
				else
					return null;
		}

		protected Version GetVersion(string paramenter, bool mandatory = true)
		{
			var p = One(paramenter);

			if(p != null)
				return Version.Parse(p.Get<string>());
			else
				if(mandatory)
					throw new SyntaxException($"Parameter '{paramenter}' not provided");
				else
					return null;
		}

		protected string GetString(string paramenter, string def)
		{
			var p = One(paramenter);

			if(p != null)
				return p.Get<string>();
			else
				return def;
		}

		protected Money GetMoney(string paramenter)
		{
			var p = One(paramenter);

			if(p != null)
				return Money.ParseDecimal(p.Get<string>());
			else
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
		}

		protected Money GetMoney(string paramenter, Money def)
		{
			var p = One(paramenter);

			if(p != null)
				return Money.ParseDecimal(p.Get<string>());
			else
				return def;
		}

		protected bool HasData()
		{
			return Has("raw") || Has("consil") || Has("analysis");
		}

		protected ResourceData GetData()
		{
			if(Has("raw"))
				return Args.Find(i => i.Name == "raw").Value != null && GetBytes("raw").Length > 0  ? new ResourceData(new BinaryReader(new MemoryStream(GetBytes("raw")))) : null;

			if(Has("consil"))
				return new ResourceData(DataType.Consil, new Consil {Analyzers = GetString("consil/analyzers").Split(',').Select(i => AccountAddress.Parse(i)).ToArray(),  
																	 PerByteFee = GetMoney("consil/fee") });
			if(Has("analysis"))
				return new ResourceData(DataType.Analysis, new Analysis {Release = GetReleaseAddress("analysis/release"), 
																		 Payment = GetMoney("analysis/payment"),
																		 Consil  = GetResourceAddress($"analysis/consil")});
			return null;
		}

		protected E GetEnum<E>(string paramenter, E def) where E : struct
		{
			var p = One(paramenter);

			if(p != null)
				return Enum.Parse<E>(p.Get<string>());
			else
				return def;
		}

		protected E GetEnum<E>(string paramenter) where E : struct
		{
			var p = One(paramenter);

			if(p != null)
				return Enum.Parse<E>(p.Get<string>());
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

		public void Dump(object o, Type type = null)
		{
			void dump(string name, object value, int tab)
			{
				if(value is null)
				{
					Workflow.Log?.Report(new string(' ', tab * 3) + name + " :");
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

			foreach(var i in o.GetType().GetProperties().Where(i => i.CanRead && i.CanWrite && i.SetMethod.IsPublic && (type == null || i.DeclaringType == type)))
			{
				dump(i.Name, i.GetValue(o), 1);
			}
		}

		public void Dump<R>(IEnumerable<R> items, string[] columns, IEnumerable<Func<R, object>> gets)
		{
			Dump(items, columns, gets.Select(g => new Func<R, int, object>((o, i) => g(o))));
		}

		public void Dump<T>(IEnumerable<T> items, string[] columns, IEnumerable<Func<T, int, object>> gets)
		{
			if(!items.Any())
			{	
				Workflow.Log?.Report("   No results");
				return;
			}


			object[,] t = new object[items.Count(), columns.Length];
			int[] w = columns.Select(i => i.Length).ToArray();

			var ii = 0;

			foreach(var i in items)
			{
				var gi = 0;

				foreach(var g in gets)
				{
					t[ii, gi] = g(i, ii);
					w[gi] = Math.Max(w[gi], t[ii, gi].ToString().Length);

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
			document.Dump((n, l) => Workflow.Log?.Report(this, new string(' ', (l+1) * 3) + n.Name + (n.Value == null ? null : (" = "  + n.Serializator.Get<string>(n, n.Value)))));
		}

		public static TransactionStatus GetAwaitStage(IEnumerable<Xon> args)
		{
			var a = args.FirstOrDefault(i => i.Name == AwaitArg);

			if(a != null)
			{
				return Enum.GetValues<TransactionStatus>().First(i => i.ToString().ToLower() == a.Get<string>());
			}
			else
				return TransactionStatus.Placed;
		}
	}
}
