using System;
using System.IO;
using System.Net;

namespace Uccs.Net
{
	public class SunJsonApiServer : JsonApiServer
	{
		Sun Sun;

		public SunJsonApiServer(Sun sun, Workflow workflow): base(	sun.Settings.Profile,
																	sun.Settings.JsonServerListenAddress, 
																	sun.Settings.Api.AccessKey, 
																	SunJsonApiClient.DefaultOptions,
																	workflow)
		{
			Sun = sun;
		}

		protected override Type Create(string call)
		{
			return Type.GetType(typeof(SunJsonApiServer).Namespace + '.' + call);
		}

		protected override object Execute(object call, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			return (call as SunApc).Execute(Sun, request, response, workflow);
		}
	}
}
