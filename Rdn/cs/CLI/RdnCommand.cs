using System.Diagnostics;
using System.Reflection;

namespace Uccs.Rdn.CLI
{
	public abstract class RdnCommand : NetCommand
	{
		public const string			AwaitArg = "await";
		public Action				Transacted;
		protected Program			Program;
		protected override Type[]	TypesForExpanding => [	typeof(IEnumerable<Dependency>), 
															typeof(IEnumerable<AnalyzerResult>), 
															typeof(Resource), 
															typeof(Manifest)];
		static RdnCommand()
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

		protected RdnCommand(Program program, List<Xon> args, Flow flow) : base(args, flow)
		{
			Program = program;
		}

		protected void ReportPreambule()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

			Flow.Log.Report(fvi.ProductName);
			Flow.Log.Report(fvi.FileVersion);
		}

		protected void ReportNetwork()
		{
			Flow.Log.Report($"Current Zone    : {Program.Zone}");
		}

		public void Api(Apc call)
		{
			if(Has("apitimeout"))
				call.Timeout = GetInt("apitimeout") * 1000;

			if(Program.ApiClient == null) 
			{	
				if(call is NodeApc s)
				{
					s.Execute(Program.Node, null, null, Flow);
					return;
				}

				throw new Exception();
			}
			else
			{	
				Program.ApiClient.Send(call, Flow);
			}
		}

		public Rp Api<Rp>(Apc call)
		{
			if(Has("apitimeout"))
				call.Timeout = GetInt("apitimeout") * 1000;

			if(Program.ApiClient == null) 
			{	
				if(call is NodeApc n)	return (Rp)n.Execute(Program.Node, null, null, Flow);

				throw new Exception();
			}
			else
			{	
				return Program.ApiClient.Request<Rp>(call, Flow);
			}
		}

		public Rp Rdc<Rp>(PeerCall<Rp> call) where Rp : PeerResponse
		{
			if(Program.ApiClient == null) 
			{
				return Program.Node.Call(() => call, Flow);
			}
			else
			{
				var rp = Api<Rp>(new PeerRequestApc {Request = call});
 
 				if(rp.Error != null)
 					throw rp.Error;
 
				return rp;
			}
		}

		public object Transact(IEnumerable<Operation> operations, AccountAddress by, TransactionStatus await)
		{
			if(Program.ApiClient == null)
				 return Program.Node.Transact(operations, by, await, Flow);
			else
				return Program.ApiClient.Request<string[][]>(new TransactApc{Operations = operations,
																			 By = by,
																			 Await = await},
															Flow);
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

		protected Ura GetResourceAddress(string paramenter, bool mandatory = true)
		{
			if(Has(paramenter))
				return Ura.Parse(GetString(paramenter));
			else
				if(mandatory)
					throw new SyntaxException($"Parameter '{paramenter}' not provided");
				else
					return null;
		}

		protected Urr GetReleaseAddress(string paramenter, bool mandatory = true)
		{
			if(Has(paramenter))
				return Urr.Parse(GetString(paramenter));
			else
				if(mandatory)
					throw new SyntaxException($"Parameter '{paramenter}' not provided");
				else
					return null;
		}

		protected Money GetMoney(string paramenter)
		{
			var p = One(paramenter);

			if(p != null)
				return Money.Parse(p.Get<string>());
			else
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
		}

		protected Money GetMoney(string paramenter, Money def)
		{
			var p = One(paramenter);

			if(p != null)
				return Money.Parse(p.Get<string>());
			else
				return def;
		}

		protected ResourceData GetData()
		{
			var d = One("data");

			if(d != null)
			{
				if(d.Nodes.Any())
				{
					var t = GetEnum<DataType>("data");
					
					switch(t)
					{
						case DataType.Raw:
							return new ResourceData(t, d.Get<string>("bytes").FromHex());

						case DataType.File:
						case DataType.Directory:
						case DataType.Package:
							return new ResourceData(t, Urr.Parse(d.Get<string>("address")));
				
						case DataType.Consil:
							return new ResourceData(t, new Consil  {Analyzers = d.Get<string>("analyzers").Split(',').Select(AccountAddress.Parse).ToArray(),  
																	PerByteSTFee = d.Get<Money>("pbstf") });
						case DataType.Analysis:
							return new ResourceData(t, new Analysis {Release = Urr.Parse(d.Get<string>("release")), 
																	 STPayment = d.Get<Money>("stpayment"),
																	 EUPayment = d.Get<Money>("eupayment"),
																	 Consil  = d.Get<Ura>("consil")});
						default:
							throw new SyntaxException("Unknown type");
					}
				}
				else if(d.Value != null)
					return new ResourceData(new BinaryReader(new MemoryStream(GetBytes("data"))));
			}

			return null;
		}

	}
}
