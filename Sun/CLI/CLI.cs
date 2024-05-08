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
		public Flow			Flow = new Flow("CLI", new Log());
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
				Flow.Log.ReportError(null, "Execution aborted");
			}
			catch(Exception ex) when(!Debugger.IsAttached)
			{
				if(Command.ConsoleAvailable)
				{
					Console.WriteLine(ex.ToString());

					if(ex is ApiCallException ace)
					{
						Console.WriteLine(ace.Response.Content.ReadAsStringAsync().Result);
					}
				}

				Directory.CreateDirectory(b.Profile);
				File.WriteAllText(Path.Join(b.Profile, "CLI." + Net.Sun.FailureExt), ex.ToString());
			}

			Sun?.Stop("The End");
		}

		public Program(Zone zone, Net.Sun sun, JsonApiClient api, Flow workflow, IPasswordAsker passwordAsker)
		{
			Zone = zone;
			Sun = sun;
			ApiClient = api;
			Flow = workflow;
			PasswordAsker = passwordAsker;
		}

		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = 
			Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

			new Program();
		}

		public Command Create(IEnumerable<Xon> commnad, Log log)
		{
			Command c;
			var t = commnad.First().Name;

			var args = commnad.Skip(1).ToList();

			var f = Flow.CreateNested("Command", log ?? Flow.Log);

			switch(t)
			{
				case BatchCommand.Keyword :		c = new BatchCommand(this, args, f); break;
				case AccountCommand.Keyword :	c = new AccountCommand(this, args, f); break;
				case NodeCommand.Keyword:		c = new NodeCommand(this, args, f); break;
				case AnalysisCommand.Keyword:	c = new AnalysisCommand(this, args, f); break;
				case DevCommand.Keyword:		c = new DevCommand(this, args, f); break;
				case WalletCommand.Keyword:		c = new WalletCommand(this, args, f); break;
				case MoneyCommand.Keyword:		c = new MoneyCommand(this, args, f); break;
				case NexusCommand.Keyword:		c = new NexusCommand(this, args, f); break;
				case DomainCommand.Keyword:		c = new DomainCommand(this, args, f); break;
				case PackageCommand.Keyword:	c = new PackageCommand(this, args, f); break;
				case ResourceCommand.Keyword:	c = new ResourceCommand(this, args, f); break;
				case ReleaseCommand.Keyword:	c = new ReleaseCommand(this, args, f); break;
				case LogCommand.Keyword:		c = new LogCommand(this, args, f); break;
				case LinkCommand.Keyword:		c = new LinkCommand(this, args, f); break;
				default:
					throw new SyntaxException("Unknown command");
			}

			return c;
		}

		public object Execute(IEnumerable<Xon> command, Log log = null)
		{
			if(Flow.Aborted)
				throw new OperationCanceledException();

			if(command.Skip(1).FirstOrDefault()?.Name == "?")
			{
				var l = new Log();
				var v = new ConsoleLogView(false, false);
				v.StartListening(l);

				var t = GetType().Assembly.GetTypes().FirstOrDefault(i => i.BaseType == typeof(Command) && i.Name.ToLower() == command.First().Name + "command");

				var c = Activator.CreateInstance(t, [this, null, Flow.CreateNested("Help", l)]) as Command;

				foreach(var j in c.Actions)
				{
					c.Report(string.Join(", ", j.Names));
					c.Report("");
					c.Report("   Syntax      : "+ j.Help?.Syntax);
					c.Report("   Description : "+ j.Help?.Description);
					c.Report("");
				}

				v.StopListening(l);

				return c;
			}
			else if(command.Skip(2).FirstOrDefault()?.Name == "?")
			{
				var l = new Log();
				var v = new ConsoleLogView(false, false);
				v.StartListening(l);

				var t = GetType().Assembly.GetTypes().FirstOrDefault(i => i.BaseType == typeof(Command) && i.Name.ToLower() == command.First().Name + "command");

				var c = Activator.CreateInstance(t, [this, null, Flow.CreateNested("Help", l)]) as Command;

				var a = c.Actions.FirstOrDefault(i => i.Names.Contains(command.Skip(1).First().Name));

				c.Report("Syntax :");
				c.Report("");
				c.Report("   " + a.Help.Syntax);

				c.Report("");

				c.Report("Description :");
				c.Report("");
				c.Report("   " + a.Help.Description);

				if(a.Help.Arguments.Any())
				{ 
					c.Report("");

					c.Report("Arguments :");
					c.Report("");
					c.Dump(a.Help.Arguments, ["Name", "Description"], [i => i.Name, i => i.Description], 1);
				}

				v.StopListening(l);
									
				return c;
			}
			else
			{
				var c = Create(command, log);

				var a = c.Actions.FirstOrDefault(i => !i.Names.Any() || i.Names.Contains(command.Skip(1).FirstOrDefault()?.Name));

				if(a.Names.Any())
				{
					c.Args.RemoveAt(0);
				}

				var r = a.Execute();

				if(r is Operation o)
				{
					if(c.Has("estimate"))
					{
						var rp = c.Api<AllocateTransactionResponse>(new EstimateOperationApc {Operations = [o], By = c.GetAccountAddress("by")});
						c.Dump(rp);
					}
					else
					{
						c.Transact([o], c.GetAccountAddress("by"), Command.GetAwaitStage(command));
						c.Transacted?.Invoke();
					}
				}
				
				return r;
			}
		}
	}
}
