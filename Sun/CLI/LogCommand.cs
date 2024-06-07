using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	internal class LogCommand : SunCommand
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
													if(Program.Sun == null)
														throw new Exception("\"node run peer\" mode supported only");

													if(ConsoleAvailable)
													{
														var old = Program.LogView.Log;
														Program.LogView.StartListening(Program.Sun.Flow.Log);
								
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