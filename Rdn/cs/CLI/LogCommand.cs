﻿namespace Uccs.Rdn.CLI
{
	internal class LogCommand : RdnCommand
	{
		public const string Keyword = "log";

		public LogCommand(Program program, List<Xon> args, Flow flow) : base(program, args, flow)
		{
			Actions =	[
							new ()
							{
								Names = [],
								Help = new Help
								{ 
									Title = "LOG",
									Description = "Applicable in \"node run\" mode only. Start monitoring the log.",
									Syntax = "log",
								},

								Execute = () =>	{
													if(Program.Node == null)
														throw new Exception("\"node run peer\" mode supported only");

													if(ConsoleAvailable)
													{
														var old = Program.LogView.Log;
														Program.LogView.StartListening(Program.Node.Flow.Log);
								
														while(Flow.Active && !Console.KeyAvailable)
														{
															Thread.Sleep(100);
														}

														Program.LogView.StopListening();
														Program.LogView.StartListening(old);
													}

													return null;
												}
							},
						];
		}
	}
}