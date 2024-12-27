namespace Uccs.Net;

public class LogCommand : McvCommand
{
	public const string Keyword = "log";

	public LogCommand(McvCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		Actions =	[
						new ()
						{
							Names = [],
							Help = new Help
							{ 
								Title = "LOG",
								Description = "Applicable in \"node run\" mode only. Start monitoring the log.",
								Syntax = $"log",
							},

							Execute = () =>	{
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
											}
						},
					];
	}
}