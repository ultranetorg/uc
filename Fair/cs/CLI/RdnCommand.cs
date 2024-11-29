using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Uccs.Fair.CLI
{
	public abstract class FairCommand : NetCommand
	{
		public const string			AwaitArg = "await";
		public Action				Transacted;
		protected Program			Program;
		protected override Type[]	TypesForExpanding => [];
		static FairCommand()
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

		protected FairCommand(Program program, List<Xon> args, Flow flow) : base(args, flow)
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
	}
}
