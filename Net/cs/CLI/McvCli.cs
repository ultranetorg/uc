using System.Diagnostics;
using System.Reflection;

namespace Uccs.Net;

public class McvCli
{
	public string			ExeDirectory;
	public McvNet			Net;
	public TcpPeering		Node;
	public JsonClient		ApiClient;
	public IPasswordAsker	PasswordAsker = new ConsolePasswordAsker();
	public McvNodeSettings	Settings;
	public Flow				Flow = new Flow("CLI", new Log()); 
	public ConsoleLogView	LogView = new ConsoleLogView(false, true);
	
	public McvCli()
	{
		ExeDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
	
		var b = new Boot(ExeDirectory);

		if(!b.Commnand.Nodes.Any())
			return;

		try
		{
			//Sun = new Net.Sun(Net, Settings, Flow);

			var l = new Log();
			LogView.StartListening(l);

			Execute(b, Flow.CreateNested("Command", l));
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

	public McvCli(McvNodeSettings settings, McvApiClient api, Flow workflow, IPasswordAsker passwordAsker)
	{
		Settings = settings;
		ApiClient = api;
		Flow = workflow;
		PasswordAsker = passwordAsker;
	}

	public virtual McvCommand Create(IEnumerable<Xon> commnad, Flow flow)
	{
		McvCommand c = null;
		var t = commnad.First().Name;

		var args = commnad.Skip(1).ToList();

		switch(t)
		{
			case AccountCommand.Keyword :	c = new AccountCommand(this, args, flow); break;
			case BandwidthCommand.Keyword:	c = new BandwidthCommand(this, args, flow); break;
			case BatchCommand.Keyword :		c = new BatchCommand(this, args, flow); break;
			case LogCommand.Keyword:		c = new LogCommand(this, args, flow); break;
			case NodeCommand.Keyword:		c = new NodeCommand(this, args, flow); break;
		}

		return c;
	}

	public virtual object Execute(Boot boot, Flow flow)
	{
		return Execute(boot.Commnand.Nodes, flow);
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

			var t = GetType().Assembly.GetTypes().FirstOrDefault(i => i.BaseType == typeof(McvCommand) && i.Name.ToLower() == command.First().Name + "command");

			var c = Activator.CreateInstance(t, [this, null, flow]) as McvCommand;

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
			var t = GetType().Assembly.GetTypes().FirstOrDefault(i => i.BaseType == typeof(McvCommand) && i.Name.ToLower() == command.First().Name + "command");

			var c = Activator.CreateInstance(t, [this, null, flow]) as McvCommand;

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
					var txs = c.Transact([o], c.GetAccountAddress("signer"), McvCommand.GetAwaitStage(command));

					foreach(var i in txs)
					{	
						if(i.Status != TransactionStatus.FailedOrNotFound)
							c.Dump(i);
						else
							c.Flow.Log?.Report($"   {nameof(ApcTransaction.Status)} : {i.Status}");
					}

					c.Transacted?.Invoke();
				}
			}
			
			return r;
		}
	}
}
