using System;
using System.Net.Http;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	internal class RunCommand : Command
	{
		public const string Keyword = "run";
		HttpClient Http = new HttpClient(); 

		public RunCommand(Zone zone, Settings settings, Workflow workflow, Net.Sun sun, Xon args) : base(zone, settings, workflow, sun, args)
		{
		}

		public override object Execute()
		{
			Sun.Run(Args, Workflow);

			ApiClient = new JsonClient(Http, $"http://{Sun.Settings.IP}:{Sun.Settings.JsonServerPort}", Sun.Settings.Api.AccessKey);

			if(ConsoleAvailable)
			{	
				//Console.ReadKey(true);
				//Wait(() => Workflow.Active && !Console.KeyAvailable);
				//
				//if(Workflow.Active)
				//{
				//}

				CLI.Program.LogView.Tags = new string[] {};

				string c;

				//if(Sun.Nuid != Guid.Empty)
				//{
				//	while(!Sun.MinimalPeersReached)
				//		Thread.Sleep(100);
				//}

				while(true)
				{
					Console.Write(">");
					c = Console.ReadLine();

					if(c == "exit")
						break;

					try
					{
						var xc = new XonDocument(c);
	
						Program.Execute(xc, Program.Boot.Zone, Settings, Sun, Workflow);
					}
					catch(Exception ex)
					{
						Workflow.Log.ReportError(this, "Error", ex);
					}
				}
			
				//Sun.Stop("By user input");
			}
			else
				Wait(() => Workflow.Active);
			
			return Sun;
		}
	}
}
