using System.Diagnostics;
using System.Reflection;

namespace Uccs.Rdn.CLI
{
	public class Program
	{
		public string			ExeDirectory;
		public Rdn				Net;
		internal RdnTcpPeering	Node;
		public JsonClient		ApiClient;
		public IPasswordAsker	PasswordAsker = new ConsolePasswordAsker();
		public RdnNodeSettings	Settings;
		public Flow				Flow = new Flow("CLI", new Log()); 
		public ConsoleLogView	LogView = new ConsoleLogView(false, true);

		public Program()
		{
			ExeDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
		
			var b = new Boot(ExeDirectory);
			Settings = new RdnNodeSettings(b.Profile);

			if(!b.Commnand.Nodes.Any())
				return;

			Net = Rdn.ByZone(b.Zone);

			try
			{
				//Sun = new Net.Sun(Net, Settings, Flow);

				var l = new Log();
				LogView.StartListening(l);

				Execute(b.Commnand.Nodes, Flow.CreateNested("Command", l));
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
				File.WriteAllText(Path.Join(b.Profile, "CLI." + Uccs.Net.Node.FailureExt), ex.ToString());
			}

			LogView.StopListening();

			//Sun?.Stop("The End");
		}

		public Program(RdnNodeSettings settings, RdnApiClient api, Flow workflow, IPasswordAsker passwordAsker)
		{
			Settings = settings;
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

		public RdnCommand Create(IEnumerable<Xon> commnad, Flow f)
		{
			RdnCommand c;
			var t = commnad.First().Name;

			var args = commnad.Skip(1).ToList();

			switch(t)
			{
				case BatchCommand.Keyword :		c = new BatchCommand(this, args, f); break;
				case AccountCommand.Keyword :	c = new AccountCommand(this, args, f); break;
				case NodeCommand.Keyword:		c = new NodeCommand(this, args, f); break;
				case AnalysisCommand.Keyword:	c = new AnalysisCommand(this, args, f); break;
				case DevCommand.Keyword:		c = new DevCommand(this, args, f); break;
				case EconomyCommand.Keyword:	c = new EconomyCommand(this, args, f); break;
				case DomainCommand.Keyword:		c = new DomainCommand(this, args, f); break;
				case ResourceCommand.Keyword:	c = new ResourceCommand(this, args, f); break;
				case ReleaseCommand.Keyword:	c = new ReleaseCommand(this, args, f); break;
				case LogCommand.Keyword:		c = new LogCommand(this, args, f); break;
				case LinkCommand.Keyword:		c = new LinkCommand(this, args, f); break;
				case BandwidthCommand.Keyword:	c = new BandwidthCommand(this, args, f); break;
				default:
					throw new SyntaxException("Unknown command");
			}

			return c;
		}

		public object Execute(IEnumerable<Xon> command, Flow flow)
		{
			if(Flow.Aborted)
				throw new OperationCanceledException();

			if(command.Skip(1).FirstOrDefault()?.Name == "?")
			{
				var l = new Log();
				var v = new ConsoleLogView(false, false);
				v.StartListening(l);

				var t = GetType().Assembly.GetTypes().FirstOrDefault(i => i.BaseType == typeof(RdnCommand) && i.Name.ToLower() == command.First().Name + "command");

				var c = Activator.CreateInstance(t, [this, null, flow]) as RdnCommand;

				foreach(var j in c.Actions)
				{
					c.Report(string.Join(", ", j.Names));
					c.Report("");
					c.Report("   Syntax      : "+ j.Help?.Syntax);
					c.Report("   Description : "+ j.Help?.Description);
					c.Report("");
				}

				return c;
			}
			else if(command.Skip(2).FirstOrDefault()?.Name == "?")
			{
				var t = GetType().Assembly.GetTypes().FirstOrDefault(i => i.BaseType == typeof(RdnCommand) && i.Name.ToLower() == command.First().Name + "command");

				var c = Activator.CreateInstance(t, [this, null, flow]) as RdnCommand;

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
									
				return c;
			}
			else
			{
				var c = Create(command, flow);

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
						var rp = c.Api<AllocateTransactionResponse>(new EstimateOperationApc {Operations = [o], By = c.GetAccountAddress("signer")});
						c.Dump(rp);
					}
					else
					{
						var x = c.Transact([o], c.GetAccountAddress("signer"), RdnCommand.GetAwaitStage(command));

						if(x is string[][] logs)
						{
							foreach(var i in logs)
								foreach(var j in i)
									c.Flow.Log?.Report(j);
						}

						c.Transacted?.Invoke();
					}
				}
				
				return r;
			}
		}
	}
}
