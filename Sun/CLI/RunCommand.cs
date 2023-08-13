using System;
using System.IO;
using System.Reflection;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	internal class RunCommand : Command
	{
		public const string Keyword = "run";

		public RunCommand(Zone zone, Settings settings, Log log, Func<Net.Sun> sun, Xon args) : base(zone, settings, log, sun, args)
		{
		}

		public override object Execute()
		{
			if(ConsoleSupported)
				Workflow.Log?.ReportWarning(this, "Pressing any key stops the node");

			Workflow.Log.Stream = new FileStream(Path.Combine(Settings.Profile, "Log.txt"), FileMode.Create);

			if(Args.Has("api"))
				Sun.RunApi();

			if(Args.Has("node"))
				Sun.RunNode();

			if(ConsoleSupported)
			{	
				//Console.ReadKey(true);
				Wait(() => !Sun.Workflow.IsAborted && !Console.KeyAvailable);

				if(!Sun.Workflow.IsAborted)
				{
					Sun.Stop("By user input");
				}
			}
			else
				Wait(() => !Sun.Workflow.IsAborted);
			
			return Sun;
		}
	}
}
