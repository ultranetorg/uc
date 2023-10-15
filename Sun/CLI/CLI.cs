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
		public JsonApiClient	Api;
		public Workflow			Workflow = new Workflow("CLI", new Log());

		public Program()
		{
			ExeDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

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

		public Program(Zone zone, Net.Sun sun, JsonApiClient api, Workflow workflow)
		{
			Zone = zone;
			Sun = sun;
			Api = api;
			Workflow = workflow;
		}

		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = 
			Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

			new Program();
		}

		Command Create(Xon commnad)
		{
			Command c = null;
			var t = commnad.Nodes.First().Name;

			var args = new Xon {Nodes = commnad.Nodes.Skip(1).ToList()};

			switch(t)
			{
				case RunCommand.Keyword:		c = new RunCommand(this, args); break;
				case AttachCommand.Keyword:		c = new AttachCommand(this, args); break;
				case DevCommand.Keyword:		c = new DevCommand(this, args); break;
				case AccountCommand.Keyword:	c = new AccountCommand(this, args); break;
				case MoneyCommand.Keyword:		c = new MoneyCommand(this, args); break;
				case NexusCommand.Keyword:		c = new NexusCommand(this, args); break;
				case AuthorCommand.Keyword:		c = new AuthorCommand(this, args); break;
				case PackageCommand.Keyword:	c = new PackageCommand(this, args); break;
				case ResourceCommand.Keyword:	c = new ResourceCommand(this, args); break;
				case NetCommand.Keyword:		c = new NetCommand(this, args); break;
			}

			return c;
		}


		public object Execute(Xon command)
		{
			if(Workflow.Aborted)
				throw new OperationCanceledException();

			var args = command.Nodes.ToList();
			var c = Create(command);

			if(c != null)
			{
				var a = c.Execute();

				if(a is Operation o)
				{
					Enqueue(new Operation[]{o}, c.GetAccountAddress("by"), Command.GetAwaitStage(command));
					//if(api == null)
					//	c.Sun.Enqueue(new Operation[]{o}, c.Sun.Vault.GetKey(c.GetAccountAddress("by")), Command.GetAwaitStage(command),  workflow);
					//else
					//	api.Send(new EnqeueOperationCall{	By = c.GetAccountAddress("by"),
					//										Await = Command.GetAwaitStage(command),
					//										Operations = new [] {o}}, 
					//				Workflow);
				}
				
				return a;
			} 
			else
			{
				var results = command.Nodes.Where(i => i.Name != "await" && i.Name != "by").Select(i => Create(i)).Select(i => i.Execute());

				Enqueue(results.OfType<Operation>(), AccountAddress.Parse(command.Get<string>("by")), Command.GetAwaitStage(command));

				return results;
			}
		}

		public Rp Call<Rp>(SunApiCall call)
		{
			if(Api == null) 
				return (Rp)call.Execute(Sun, Workflow);
			else
				return Api.Request<Rp>(call, Workflow);
		}

		public Rp Rdc<Rp>(RdcRequest request) where Rp : RdcResponse
		{
			return Call<Rp>(new RdcCall {Request = request});
		}

		public void Call(SunApiCall call)
		{
			if(Api == null)
				call.Execute(Sun, Workflow);
			else
				Api.Send(call, Workflow);
		}

		public void Enqueue(IEnumerable<Operation> operations, AccountAddress by, PlacingStage await)
		{
			if(Api == null)
				Sun.Enqueue(operations, by, await, Workflow);
			else
				Api.Send(new EnqeueOperationCall{	Operations = operations,
													By = by,
													Await = await}, 
							Workflow);
		}
	}
}
