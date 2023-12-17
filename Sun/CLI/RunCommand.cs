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

			var settings = new Settings(Program.ExeDirectory, b);
				
			if(File.Exists(settings.Profile))
				foreach(var i in Directory.EnumerateFiles(settings.Profile, "*." + Net.Sun.FailureExt))
					File.Delete(i);

			Program.Sun = new Net.Sun(b.Zone, settings){Clock = new RealTimeClock(),
														Nas = new Nas(settings),
														GasAsker = Command.ConsoleAvailable ? new ConsoleGasAsker() : new SilentGasAsker(),
														FeeAsker = new SilentFeeAsker() };
			
			Program.Sun.Run(Args, Workflow);

			WaitHandle.WaitAny(new WaitHandle[] {Workflow.Cancellation.WaitHandle});
			
			return null;
		}
	}
}
