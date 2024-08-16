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
															typeof(VersionManifest)];
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
																			 Signer = by,
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

		protected Unit GetMoney(string paramenter)
		{
			var p = One(paramenter);

			if(p != null)
				return Unit.Parse(p.Get<string>());
			else
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
		}

		protected Unit GetMoney(string paramenter, Unit def)
		{
			var p = One(paramenter);

			if(p != null)
				return Unit.Parse(p.Get<string>());
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
					var ctl = DataType.Parse(GetString("data"));
					var cnt = GetString("data/type", false) is string a ? ContentType.Parse(a) : null;
					var t = new DataType(ctl, cnt);

					if(ctl == DataType.Self)
					{	
						if(cnt == ContentType.Unknown)
							return new ResourceData(t, d.Get<string>("hex").FromHex());
				
						if(cnt == ContentType.Rdn_Consil)
							return new ResourceData(t, new Consil{	Analyzers = d.Get<string>("analyzers").Split(',').Select(AccountAddress.Parse).ToArray(),  
																	PerByteBYFee = d.Get<Unit>("pbstf") });
						
						if(cnt == ContentType.Rdn_Analysis)
							return new ResourceData(t, new Analysis{Release		= Urr.Parse(d.Get<string>("release")), 
																	BYPayment	= d.Get<Unit>("stpayment"),
																	ECPayment	= d.Get<Unit>("eupayment"),
																	Consil		= d.Get<Ura>("consil")});
					}
					else
					{
						if(	ctl == DataType.File ||
							ctl == DataType.Directory)
							return new ResourceData(t, Urr.Parse(d.Get<string>("address")));
					}

					throw new SyntaxException("Unknown type");
				}
				else if(d.Value != null)
					return new ResourceData(new BinaryReader(new MemoryStream(GetBytes("data"))));
			}

			return null;
		}

	}
}
