using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Uccs.Rdn.CLI
{
	public abstract class RdnCommand : NetCommand
	{
		public const string			AwaitArg = "await";
		public Action				Transacted;
		protected Program			Program;
		protected override Type[]	TypesForExpanding => [typeof(IEnumerable<Dependency>), 
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
			Flow.Log.Report($"Current Net    : {Program.Net}");
		}

		public void Api(Apc call)
		{
			if(Has("apitimeout"))
				call.Timeout = GetInt("apitimeout") * 1000;

			Program.ApiClient.Send(call, Flow);
		}

		public Rp Api<Rp>(Apc call)
		{
			if(Has("apitimeout"))
				call.Timeout = GetInt("apitimeout") * 1000;

			return Program.ApiClient.Request<Rp>(call, Flow);
		}

		public Rp Rdc<Rp>(Ppc<Rp> call) where Rp : PeerResponse
		{
			var rp = Api<Rp>(new PeerRequestApc {Request = call});
 
 			if(rp.Error != null)
 				throw rp.Error;
 
			return rp;
		}

		public ApcTransaction[] Transact(IEnumerable<Operation> operations, AccountAddress signer, TransactionStatus await)
		{
			return Program.ApiClient.Request<ApcTransaction[]>(new TransactApc {Operations = operations,
																				Signer = signer,
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

		protected long GetMoney(string paramenter)
		{
			var p = One(paramenter);

			if(p != null)
				return long.Parse(p.Get<string>(), NumberStyles.AllowThousands);
			else
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
		}

		protected long GetMoney(string paramenter, long def)
		{
			var p = One(paramenter);

			if(p != null)
				return long.Parse(p.Get<string>(), NumberStyles.AllowThousands);
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

					if(ctl == DataType.Data)
					{	
						if(cnt == ContentType.Unknown)
							return new ResourceData(t, d.Get<string>("hex").FromHex());
				
						if(cnt == ContentType.Rdn_Consil)
							return new ResourceData(t, new Consil{	Analyzers = d.Get<string>("analyzers").Split(',').Select(AccountAddress.Parse).ToArray(),  
																	PerByteBYFee = d.Get<long>("pbstf") });
						
						if(cnt == ContentType.Rdn_Analysis)
							return new ResourceData(t, new Analysis{Release		= Urr.Parse(d.Get<string>("release")), 
																	ECPayment	= [new (Time.Zero, d.Get<long>("expayment"))],
																	BYPayment	= d.Get<long>("stpayment"),
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
