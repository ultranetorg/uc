using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	public class Program
	{
		public ConsoleLogView	LogView;
		public string			ExeDirectory;
		public Zone				Zone;
		public Net.Sun			Sun;
		public JsonApiClient	ApiClient;
		public Workflow			Workflow = new Workflow("CLI", new Log());
		public IPasswordAsker	PasswordAsker;

		public Program()
		{
// 			double r = 17000;
// 			double pp = 17000/20;
// 			double s = 0;
// 
// 			for(int i=0; i<20; i++)
// 			{
// 
// 				s += pp + r * 0.07;
// 			
// 				Console.WriteLine($"год {i} - платеэж {pp + r * 0.07:0.} - всего {s:0.}");
// 				
// 				r -= pp;
// 			}


			ExeDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			PasswordAsker = new ConsolePasswordAsker();

			try
			{
				var p = Console.KeyAvailable;
				LogView = new ConsoleLogView(Workflow.Log, false, true);
			}
			catch(Exception)
			{
			}

			try
			{
				foreach(var i in Directory.EnumerateFiles(ExeDirectory, "*." + Net.Sun.FailureExt))
					File.Delete(i);
				
				var b = new Boot(ExeDirectory);

				if(!b.Commnand.Nodes.Any())
					return;

				Execute(b.Commnand);
			}
			catch(OperationCanceledException)
			{
				Workflow.Log.ReportError(null, "Execution aborted");
			}
			catch(Exception ex) when(!Debugger.IsAttached)
			{
				var m = Path.GetInvalidFileNameChars().Aggregate(MethodBase.GetCurrentMethod().Name, (c1, c2) => c1.Replace(c2, '_'));
				File.WriteAllText(Path.Join(ExeDirectory, m + "." + Net.Sun.FailureExt), ex.ToString());
			}

			Sun.Stop("The End");
		}

		public Program(Zone zone, Net.Sun sun, JsonApiClient api, Workflow workflow, IPasswordAsker passwordAsker)
		{
			Zone = zone;
			Sun = sun;
			ApiClient = api;
			Workflow = workflow;
			PasswordAsker = passwordAsker;
		}

		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = 
			Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

			new Program();
		}

		public Command Create(Xon commnad)
		{
			Command c = null;
			var t = commnad.Nodes.First().Name;

			var args = new Xon {Nodes = commnad.Nodes.Skip(1).ToList()};

			switch(t)
			{
				case BatchCommand.Keyword :		c = new BatchCommand(this, args); break;
				case RunCommand.Keyword:		c = new RunCommand(this, args); break;
				case AttachCommand.Keyword:		c = new AttachCommand(this, args); break;
				case AnalysisCommand.Keyword:	c = new AnalysisCommand(this, args); break;
				case DevCommand.Keyword:		c = new DevCommand(this, args); break;
				case AccountCommand.Keyword:	c = new AccountCommand(this, args); break;
				case MoneyCommand.Keyword:		c = new MoneyCommand(this, args); break;
				case NexusCommand.Keyword:		c = new NexusCommand(this, args); break;
				case AuthorCommand.Keyword:		c = new AuthorCommand(this, args); break;
				case PackageCommand.Keyword:	c = new PackageCommand(this, args); break;
				case ResourceCommand.Keyword:	c = new ResourceCommand(this, args); break;
				case ReleaseCommand.Keyword:	c = new ReleaseCommand(this, args); break;
				case NetCommand.Keyword:		c = new NetCommand(this, args); break;
				default:
					throw new SyntaxException("Unknown command");
			}

			return c;
		}


		public object Execute(Xon command)
		{
			if(Workflow.Aborted)
				throw new OperationCanceledException();

			var args = command.Nodes.ToList();
			var c = Create(command);

			var a = c.Execute();

			if(a is Operation o)
			{
				Enqueue(new Operation[]{o}, c.GetAccountAddress("by"), Command.GetAwaitStage(command));
			}
				
			return a;
		}

		public void Api(SunApiCall call)
		{
			if(ApiClient == null)
				call.Execute(Sun, Workflow);
			else
				ApiClient.Send(call, Workflow);
		}

		public Rp Api	<Rp>(SunApiCall call)
		{
			if(ApiClient == null) 
				return (Rp)call.Execute(Sun, Workflow);
			else
				return ApiClient.Request<Rp>(call, Workflow);
		}

		public Rp Rdc<Rp>(RdcRequest request) where Rp : RdcResponse
		{
			var rp = Api<Rp>(new RdcCall {Request = request});
 
 			if(rp.Result != RdcResult.Success)
 			{
 				string m = rp.Result.ToString();
 
 				if(rp.Result == RdcResult.EntityException)
 					m +=  " : " + ((RdcEntityError)rp.Error).ToString();
 				else if(rp.Result == RdcResult.NodeException)
 					m +=  " : " + ((RdcNodeError)rp.Error).ToString();
 
 				//if(rp.ErrorDetails != null)
 				//	m += " - " + rp.ErrorDetails;
 
 				throw new Exception(m);
 			}

			return rp;
		}

		public void Enqueue(IEnumerable<Operation> operations, AccountAddress by, PlacingStage await)
		{
			if(ApiClient == null)
				Sun.Enqueue(operations, by, await, Workflow);
			else
				ApiClient.Send(new EnqeueOperationCall {Operations = operations,
														By = by,
														Await = await}, 
								Workflow);
		}
	}
}
