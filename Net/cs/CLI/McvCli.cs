using System.Diagnostics;
using System.Reflection;

namespace Uccs.Net;

public class McvCli : Cli
{
	public string			ExeDirectory;
	public McvNet			Net;
	public TcpPeering		Node;
	public JsonClient		ApiClient;
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

	public McvCli(McvNodeSettings settings, McvApiClient api, Flow workflow)
	{
		Settings = settings;
		ApiClient = api;
		Flow = workflow;
	}

	public override Command Create(IEnumerable<Xon> commnad, Flow flow)
	{
		var t = commnad.First().Name;

		var args = commnad.Skip(1).ToList();

		var ct = Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(Command))).FirstOrDefault(i => i.Name.ToLower() == t + nameof(Command).ToLower());

		return ct?.GetConstructor([typeof(McvCli), typeof(List<Xon>), typeof(Flow)]).Invoke([this, args, flow]) as McvCommand;

		//switch(t)
		//{
		//	case AccountCommand.Keyword :	c = new AccountCommand(this, args, flow); break;
		//	case BandwidthCommand.Keyword:	c = new BandwidthCommand(this, args, flow); break;
		//	case BatchCommand.Keyword :		c = new BatchCommand(this, args, flow); break;
		//	case LogCommand.Keyword:		c = new LogCommand(this, args, flow); break;
		//	case NodeCommand.Keyword:		c = new NodeCommand(this, args, flow); break;
		//}

		//return c;
	}

	public virtual object Execute(Boot boot, Flow flow)
	{
		return Execute(boot.Commnand.Nodes, flow);
	}

	public override void PostExecute(IEnumerable<Xon> args, Command command, object result)
	{
		var c = command as McvCommand;

		if(result is Operation o)
		{
			if(c.Has("estimate"))
			{
				var rp = c.Api<AllocateTransactionResponse>(new EstimateOperationApc {Operations = [o], By = c.GetAccountAddress(McvCommand.SignerArg)});
				Flow.Log.Dump(rp);
			}
			else
			{
				var t = c.Transact([o], c.GetAccountAddress(McvCommand.SignerArg), McvCommand.GetAwaitStage(args));

				c.Transacted?.Invoke();
			}
		}
		else if(result is IEnumerable<Operation> ooo)
		{
			if(c.Has("estimate"))
			{
				var rp = c.Api<AllocateTransactionResponse>(new EstimateOperationApc {Operations = ooo, By = c.GetAccountAddress(McvCommand.SignerArg)});
				Flow.Log.Dump(rp);
			}
			else
			{
				var t = c.Transact(ooo, c.GetAccountAddress(McvCommand.SignerArg), McvCommand.GetAwaitStage(args));

				c.Transacted?.Invoke();
			}
		}
	}
}
