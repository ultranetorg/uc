using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Nethereum.ABI.Util;
using Uccs.Net;

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

		public Guid Mcvid
		{
			get
			{
				if(Has("mcvid"))
					return Guid.Parse(GetString("mcvid"));
				else
					return Program.Settings.CliDefaultMcv;
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
			Flow.Log.Report($"Current Network : {(Mcvid == Guid.Empty ? "" : Mcvid)}");
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
				
				if(call is McvApc m)
				{
					m.Execute(Program.Node.FindMcv(Mcvid), null, null, Flow);
					return;
				}

				throw new Exception();
			}
			else
			{	
				if(call is McvApc c)
					c.Mcvid = Mcvid;

				Program.ApiClient.Send(call, Flow);
			}
		}

		public Rp Api<Rp>(Apc call)
		{
			if(Has("apitimeout"))
				call.Timeout = GetInt("apitimeout") * 1000;

			if(Program.ApiClient == null) 
			{	
				if(call is NodeApc s)	return (Rp)s.Execute(Program.Node, null, null, Flow);
				if(call is McvApc m)	return (Rp)m.Execute(Program.Node.FindMcv(Mcvid), null, null, Flow);

				throw new Exception();
			}
			else
			{	
				if(call is McvApc c)
					c.Mcvid = Mcvid;

				return Program.ApiClient.Request<Rp>(call, Flow);
			}
		}

		public Rp Rdc<Rp>(PeerCall<Rp> call) where Rp : PeerResponse
		{
			if(Program.ApiClient == null) 
			{
				return Program.Node.FindMcv(Mcvid).Call(() => call, Flow);
			}
			else
			{
				var rp = Api<Rp>(new PeerRequestApc {Mcvid = Mcvid, Request = call});
 
 				if(rp.Error != null)
 					throw rp.Error;
 
				return rp;
			}
		}

		public object Transact(IEnumerable<Operation> operations, AccountAddress by, TransactionStatus await)
		{
			if(Program.ApiClient == null)
				 return Program.Node.FindMcv(Mcvid).Transact(operations, by, await, Flow);
			else
				return Program.ApiClient.Request<string[][]>(new TransactApc  {	Mcvid = Mcvid,
																				Operations = operations,
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
	}
}
