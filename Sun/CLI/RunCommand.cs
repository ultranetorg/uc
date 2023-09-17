using System;
using System.IO;
using System.Reflection;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	internal class RunCommand : Command
	{
		public const string Keyword = "run";

		public RunCommand(Zone zone, Settings settings, Workflow workflow, Func<Net.Sun> sun, Xon args) : base(zone, settings, workflow, sun, args)
		{
		}

		public override object Execute()
		{
			if(ConsoleSupported)
				Workflow.Log?.ReportWarning(this, "Pressing any key stops the node");

			if(Args.Has("api"))
				Sun.RunApi();

			if(Args.Has("node"))
				Sun.RunNode(Workflow);

			if(ConsoleSupported)
			{	
				//Console.ReadKey(true);
				Wait(() => Workflow.Active && !Console.KeyAvailable);

				if(Workflow.Active)
				{
					Sun.Stop("By user input");
				}
			}
			else
				Wait(() => Workflow.Active);
			
			return Sun;
		}
	}
}
