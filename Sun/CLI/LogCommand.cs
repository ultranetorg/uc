using System;
using System.IO;
using System.Threading;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	internal class LogCommand : Command
	{
		public const string Keyword = "log";

		public LogCommand(Program program, Xon args) : base(program, args)
		{
		}

		public override object Execute()
		{
			if(Program.Sun == null)
				throw new Exception("\"run node\" mode supported only");

			if(ConsoleAvailable)
			{
				var v = new ConsoleLogView(false, true);
				v.StartListening(Program.Sun.Workflow.Log);
								
				while(Workflow.Active && !Console.KeyAvailable)
				{
					Thread.Sleep(100);
				}

				v.StopListening(Program.Sun.Workflow.Log);
			}
			
			return null;
		}
	}
}
