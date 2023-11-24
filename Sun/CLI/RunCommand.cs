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

			//string dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

			Program.Sun = new Net.Sun(b.Zone, settings){Clock = new RealTimeClock(),
														Nas = new Nas(settings),
														GasAsker = Command.ConsoleAvailable ? new ConsoleGasAsker() : new SilentGasAsker(),
														FeeAsker = new SilentFeeAsker() };
			
			//ApiClient = new JsonClient(Http, $"http://{Sun.Settings.IP}:{Sun.Settings.JsonServerPort}", Sun.Settings.Api.AccessKey);
			//if(Boot.Commnand.Nodes.First().Name != RunCommand.Keyword)
			//{
			//	Sun.RunUser(Workflow);
			//}

			Program.Sun.Run(Args, Workflow);

			if(ConsoleAvailable)
			{	
				Program.LogView.Tags = new string[] {};

				while(true)
				{
					Console.Write(">");
					var c = Console.ReadLine();

					if(c == "exit")
						break;

					try
					{
						var xc = new XonDocument(c);
	
						Program.Execute(xc);
					}
					catch(Exception ex)
					{
						Workflow.Log.ReportError(this, "Error", ex);
					}
				}
			
				//Sun.Stop("By user input");
			}
			else
				while(Workflow.Active)
				{
					Thread.Sleep(100); 
				}
			
			return null;
		}
	}
}
