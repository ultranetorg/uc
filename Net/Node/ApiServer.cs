using System;
using System.IO;
using System.Net;

namespace Uccs.Net
{
	public class ApiJsonServer : JsonServer
	{
		Sun Sun;

		public ApiJsonServer(Sun sun, Flow workflow): base(	sun.Settings.Profile,
															sun.Settings.JsonServerListenAddress, 
															sun.Settings.Api.AccessKey, 
															ApiJsonClient.DefaultOptions,
															workflow)
		{
			Sun = sun;
		}

		protected override Type Create(string call)
		{
			return Type.GetType(typeof(ApiJsonServer).Namespace + '.' + call);
		}

		protected override object Execute(object call, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			try
			{
				if(call is SunApc s) return s.Execute(Sun, request, response, workflow);
				if(call is McvApc m) return m.Execute(Sun.FindMcv(m.Mcvid), request, response, workflow);

				throw new ApiCallException("Unknown call");
			}
			catch(SunException ex)
			{
				RespondError(response, ex.ToString(), HttpStatusCode.InternalServerError);
				return null;
			}
		}
	}
}
