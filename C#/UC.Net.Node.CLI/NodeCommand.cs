using System;
using System.IO;
using System.Reflection;

namespace UC.Net.Node.CLI
{
	internal class NodeCommand : Command
	{
		public const string Keyword = "node";

		public NodeCommand(Settings settings, Log log, Func<Core> core, Xon args) : base(settings, log, core, args)
		{
		}

		public override object Execute()
		{
			if(ConsoleSupported)
				Log.ReportWarning(this, "Pressing any key stops the node");

			if(Args.Has("server"))
			{
				Core.RunApi();
			}
			else
			{
				Core.RunApi();
				Core.RunChain();
			}

			if(ConsoleSupported)
				Console.ReadKey();
			else
				Wait(() => Core.Working);
			
			return Core;
		}
	}
}
