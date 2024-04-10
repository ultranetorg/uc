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
		public string			ExeDirectory;
		public Zone				Zone;
		public Net.Sun			Sun;
		public JsonApiClient	ApiClient;
		public Workflow			Workflow = new Workflow("CLI", new Log());
		public IPasswordAsker	PasswordAsker;

		public Program()
		{
			ExeDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			PasswordAsker = new ConsolePasswordAsker();
		
			var b = new Boot(ExeDirectory);

			if(!b.Commnand.Nodes.Any())
				return;

			Zone = b.Zone;

			try
			{
				Execute(b.Commnand.Nodes);
			}
			catch(OperationCanceledException)
			{
				Workflow.Log.ReportError(null, "Execution aborted");
			}
			catch(Exception ex) when(!Debugger.IsAttached)
			{
				if(Command.ConsoleAvailable)
					Console.WriteLine(ex.ToString());

				Directory.CreateDirectory(b.Profile);
				File.WriteAllText(Path.Join(b.Profile, "CLI." + Net.Sun.FailureExt), ex.ToString());
			}

			Sun?.Stop("The End");
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

		public Command Create(IEnumerable<Xon> commnad)
		{
			Command c;
			var t = commnad.First().Name;

			var args = commnad.Skip(1).ToList();

			switch(t)
			{
				case BatchCommand.Keyword :		c = new BatchCommand(this, args); break;
				case AccountCommand.Keyword :	c = new AccountCommand(this, args); break;
				case NodeCommand.Keyword:		c = new NodeCommand(this, args); break;
				case AnalysisCommand.Keyword:	c = new AnalysisCommand(this, args); break;
				case DevCommand.Keyword:		c = new DevCommand(this, args); break;
				case WalletCommand.Keyword:		c = new WalletCommand(this, args); break;
				case MoneyCommand.Keyword:		c = new MoneyCommand(this, args); break;
				case NexusCommand.Keyword:		c = new NexusCommand(this, args); break;
				case AuthorCommand.Keyword:		c = new AuthorCommand(this, args); break;
				case PackageCommand.Keyword:	c = new PackageCommand(this, args); break;
				case ResourceCommand.Keyword:	c = new ResourceCommand(this, args); break;
				case ReleaseCommand.Keyword:	c = new ReleaseCommand(this, args); break;
				case NetCommand.Keyword:		c = new NetCommand(this, args); break;
				case LogCommand.Keyword:		c = new LogCommand(this, args); break;
				case LinkCommand.Keyword:		c = new LinkCommand(this, args); break;
				default:
					throw new SyntaxException("Unknown command");
			}

			return c;
		}

		public object Execute(IEnumerable<Xon> command, Log log = null)
		{
			if(Workflow.Aborted)
				throw new OperationCanceledException();

			var c = Create(command);

			c.Workflow = Workflow.CreateNested("Command", Workflow.Log);
			
			if(log != null)
				c.Workflow.Log = log;

			var a = c.Execute();

			if(a is Operation o)
			{
				if(c.Has("estimate"))
				{
					var rp = c.Api<AllocateTransactionResponse>(new EstimateOperationApc { Operations = [o], By = c.GetAccountAddress("by")});
					c.Dump(rp, typeof(AllocateTransactionResponse));
				}
				else
					c.Transact([o], c.GetAccountAddress("by"), Command.GetAwaitStage(command));
			}
				
			return a;
		}
	}
}
