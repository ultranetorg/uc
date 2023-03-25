using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Uccs.Net
{
	public class Client
	{
		public Filebase		Filebase;
		public JsonClient	JsonClient;
		HttpClient			Http = new HttpClient();

		public Client(string server, string key, Zone zone, string productspath)
		{
			JsonClient = new JsonClient(Http, server, zone, key);

			var s = JsonClient.Request<SettingsResponse>(new SettingsCall(), new Workflow());

			Filebase = new Filebase(Path.Join(s.ProfilePath, nameof(Filebase)), productspath);
		}

		public void GetRelease(ReleaseAddress version, Workflow workflow)
		{
			JsonClient.Post(new GetReleaseCall {Version = version}, workflow);
		}
	}
}
