using System;
using System.IO;
using System.Reflection;

namespace UC.Net.Node.CLI
{
	internal class RunCommand : Command
	{
		public const string Keyword = "run";

		public RunCommand(Settings settings, Log log, Func<Core> core, Xon args) : base(settings, log, core, args)
		{
		}

		public override object Execute()
		{
			if(ConsoleSupported)
				Flowvizor.Log?.ReportWarning(this, "Pressing any key stops the node");

			if(Args.Has("api"))
				Core.RunApi();

			if(Args.Has("node"))
				Core.RunNode();

			if(ConsoleSupported)
			{	
				Console.ReadKey(true);
				Core.Stop("By user input");
			}
			else
				Wait(() => Core.Running);
			
			return Core;
		}
	}
}
