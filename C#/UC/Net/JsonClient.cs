using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Net;

namespace UC.Net
{
	public class JsonClient// : RpcClient
	{
		HttpClient			HttpClient;
		public string		Address;
		string				Key;
		public int			Failures;

		public static		JsonSerializerOptions Options;
		
		static JsonClient()
		{
			Options = new JsonSerializerOptions{};

			Options.IgnoreReadOnlyProperties = true;

			Options.Converters.Add(new CoinJsonConverter());
			Options.Converters.Add(new AccountJsonConverter());
			Options.Converters.Add(new IPJsonConverter());
			Options.Converters.Add(new ChainTimeJsonConverter());
			Options.Converters.Add(new ReleaseAddressJsonConverter());
			Options.Converters.Add(new XonDocumentJsonConverter());
		}

		public JsonClient(HttpClient http, string apiurl, string apikey)
		{
			HttpClient = http;
			Address = apiurl;
			Key = apikey;
		}

		HttpResponseMessage Post(RpcCall request) 
		{
			request.Version = Core.Versions.First().ToString();
			request.AccessKey = Key;

			var c = JsonSerializer.Serialize(request, request.GetType(), Options);

			var m = new HttpRequestMessage(HttpMethod.Get, Address + "/" + RpcCall.NameOf(request.GetType()));

			m.Content = new StringContent(c, Encoding.UTF8, "application/json");

			return HttpClient.Send(m);
		}

		public Rp Request<Rp>(RpcCall request)
		{
			var cr = Post(request);

			if(cr.StatusCode != System.Net.HttpStatusCode.OK)
				throw new RpcException(cr.Content.ReadAsStringAsync().Result);

			try
			{
				return JsonSerializer.Deserialize<Rp>(cr.Content.ReadAsStringAsync().Result, Options);
			}
			catch(Exception ex)
			{
				throw new RpcException("Response deserialization failed", ex);
			}
		}
		
		public void SendOnly(RpcCall request)
		{
			var cr = Post(request);
			
			if(cr.StatusCode != System.Net.HttpStatusCode.OK)
				throw new RpcException(cr.StatusCode.ToString());
		}

		public UntTransfer								Send(TransferUntCall call) => Request<UntTransfer>(call);
		public GetStatusResponse						Send(StatusCall call) => Request<GetStatusResponse>(call);
	}
}
