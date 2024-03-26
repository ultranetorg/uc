using System;
using System.IO;
using System.Net;

namespace Uccs.Net
{
	public class SunJsonApiServer : JsonApiServer
	{
		Sun Sun;

		public SunJsonApiServer(Sun sun, Workflow workflow): base(	sun.Settings.Profile,
																	sun.Settings.IP, 
																	sun.Settings.JsonServerPort, 
																	sun.Settings.Api.AccessKey, 
																	SunJsonApiClient.DefaultOptions,
																	workflow)
		{
			Sun = sun;
		}

		protected override Type Create(string call)
		{
			return Type.GetType(GetType().Namespace + '.' + call);
		}

		protected override object Execute(object call, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			return (call as SunApiCall).Execute(Sun, request, response, workflow);
		}
	}
}
