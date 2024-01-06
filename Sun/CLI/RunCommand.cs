using System;
using System.IO;
using System.Threading;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	internal class RunCommand : Command
	{
		public const string Keyword = "run";

		public RunCommand(Program program, Xon args) : base(program, args)
		{
		}

		public override object Execute()
		{
			var b = new Boot(Program.ExeDirectory);
			var s = new Settings(Program.ExeDirectory, b);

			Program.Sun = new Net.Sun(b.Zone, s, Workflow){	Clock = new RealClock(),
															Nas = new Nas(s),
															GasAsker = ConsoleAvailable ? new ConsoleGasAsker() : new SilentGasAsker(),
															FeeAsker = new SilentFeeAsker() };
			
			Program.Sun.Run(Args);

			if(ConsoleAvailable)
				while(Workflow.Active)
				{
					Console.Write(b.Zone + " > ");

					var l = new Log();
					var v = new ConsoleLogView(false, true);
					v.StartListening(l);

					try
					{
						var x = new XonDocument(Console.ReadLine());

						if(x.Nodes[0].Name == RunCommand.Keyword || x.Nodes[0].Name == AttachCommand.Keyword)
							throw new Exception("Not available");

						Program.Execute(x, l);
					}
					catch(Exception ex)
					{
						l.ReportError(this, "Error", ex);
					}

					v.StopListening(l);
				}
			else
				WaitHandle.WaitAny(new WaitHandle[] {Workflow.Cancellation.WaitHandle});
			
			return null;
		}
	}
}
