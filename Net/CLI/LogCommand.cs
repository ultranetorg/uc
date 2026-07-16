using System.Reflection;

namespace Uccs.Net;

public class LogCommand : McvCommand
{
	public LogCommand(McvCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Default()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		a.Description = "Applicable in \"node run\" mode only. Starts monitoring of the node log. Press any key to break.";

		a.Execute = () =>	{
								if(Cli.Node == null)
									throw new Exception("\"node run peer\" mode supported only");

								if(ConsoleAvailable)
								{
									var old = Cli.LogView.Log;
									Cli.LogView.StartListening(Cli.Node.Flow.Log);
							
									while(Flow.Active && !Console.KeyAvailable)
									{
										Thread.Sleep(100);
									}

									Cli.LogView.StopListening();
									Cli.LogView.StartListening(old);
								}

								return null;
							};

		return a;

	}
}