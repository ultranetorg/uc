using System;
using System.IO;
using System.Reflection;

namespace UC.Net.Node.CLI
{
	internal class NodeCommand : Command
	{
		public const string Keyword = "node";

		public NodeCommand(Settings settings, Log log, Func<Dispatcher> dispatcher, Xon args) : base(settings, log, dispatcher, args)
		{
		}

		public override object Execute()
		{
			if(ConsoleSupported)
				Log.ReportWarning(this, "Pressing any key stops the node");

			if(Args.Has("server"))
			{
				Dispatcher.RunServer();
			}
			else
			{
				Dispatcher.RunServer();
				Dispatcher.RunNode();
			}

			if(ConsoleSupported)
				Console.ReadKey();
			else
				Wait(() => Dispatcher.Working);
			
			return Dispatcher;
		}
	}
}
