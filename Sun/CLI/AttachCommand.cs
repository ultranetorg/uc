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
			var a = new Uri(GetString("to"));
			Program.ApiClient = new JsonApiClient(new HttpClient(), GetString("to"), GetString("accesskey", null));

			Program.LogView.Tags = new string[] {};

			while(true)
			{
				Console.Write($"{a.Host}:{a.Port} > ");
				var c = Console.ReadLine();

				if(c == "exit")
					break;

				try
				{
					var x = new XonDocument(c);
	
					Program.Execute(x);
				}
				catch(ApiCallException ex) when(ex.Response != null && ex.Response.StatusCode == HttpStatusCode.UnprocessableEntity)
				{
					Workflow.Log.ReportError(this, "Error", ex);
				}
				catch(Exception ex)
				{
					Workflow.Log.ReportError(this, "Error", ex);
				}
			}
			
			return null;
		}
	}
}
