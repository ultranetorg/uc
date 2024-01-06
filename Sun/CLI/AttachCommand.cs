using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	internal class AttachCommand : Command
	{
		public const string Keyword = "attach";

		public AttachCommand(Program program, Xon args) : base(program, args)
		{
		}

		public override object Execute()
		{
			var a = new Uri(Args.Nodes[0].Name);

			var h = new HttpClientHandler();
			h.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
			var http = new HttpClient(h){Timeout = TimeSpan.FromSeconds(60)};

			Program.ApiClient = new JsonApiClient(http, Args.Nodes[0].Name, GetString("accesskey", null));

			var v = new ConsoleLogView(false, true);
			v.StartListening(Workflow.Log);

			while(true)
			{
				Console.Write($"{a.Host}:{a.Port} > ");
				var c = Console.ReadLine();

				if(c == "exit")
					break;

				try
				{
					var x = new XonDocument(c);

					if(x.Nodes[0].Name == RunCommand.Keyword || x.Nodes[0].Name == AttachCommand.Keyword || x.Nodes[0].Name == LogCommand.Keyword)
						throw new Exception("Not available");
	
					Program.Execute(x);
				}
				catch(Exception ex)
				{
					Workflow.Log.ReportError(this, "Error", ex);
				}
			}

			v.StopListening(Workflow.Log);
			
			return null;
		}
	}
}
