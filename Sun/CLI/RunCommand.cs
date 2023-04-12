using System;
using System.IO;
using System.Reflection;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	internal class RunCommand : Command
	{
		public const string Keyword = "run";

		public RunCommand(Zone zone, Settings settings, Log log, Func<Core> core, Xon args) : base(zone, settings, log, core, args)
		{
		}

		public override object Execute()
		{
			if(ConsoleSupported)
				Workflow.Log?.ReportWarning(this, "Pressing any key stops the node");

			Workflow.Log.Stream = new FileStream(Path.Combine(Settings.Profile, "Log.txt"), FileMode.Create);

			if(Args.Has("api"))
				Core.RunApi();

			if(Args.Has("node"))
				Core.RunNode();

			if(ConsoleSupported)
			{	
				//Console.ReadKey(true);
				Wait(() => !Core.Workflow.IsAborted && !Console.KeyAvailable);

				if(!Core.Workflow.IsAborted)
				{
					Core.Stop("By user input");
				}
			}
			else
				Wait(() => !Core.Workflow.IsAborted);
			
			return Core;
		}
	}
}
