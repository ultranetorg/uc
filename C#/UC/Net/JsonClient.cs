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
using System.Threading;

namespace UC.Net
{
	public class ApiCallException : Exception
	{
		public ApiCallException(string msg) : base(msg){ }
		public ApiCallException(string msg, Exception ex) : base(msg, ex){ }
	}

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

		public JsonClient(HttpClient http, string server, string zone, string apikey)
		{
			HttpClient = http;
			Address = $"http://{server}:{UC.Net.Zone.JsonPort(zone)}";
			Key = apikey;
		}

		public UntTransfer			Send(TransferUntCall call, CancellationToken cancellation = default) => Request<UntTransfer>(call, cancellation );
		public GetStatusResponse	Send(StatusCall call, CancellationToken cancellation = default) => Request<GetStatusResponse>(call, cancellation);

		HttpResponseMessage Post(RpcCall request, CancellationToken cancellation = default) 
		{
			request.Version = Core.Versions.First().ToString();
			request.AccessKey = Key;

			var c = JsonSerializer.Serialize(request, request.GetType(), Options);

			var m = new HttpRequestMessage(HttpMethod.Get, Address + "/" + RpcCall.NameOf(request.GetType()));

			m.Content = new StringContent(c, Encoding.UTF8, "application/json");

			return HttpClient.Send(m, cancellation);
		}

		public Rp Request<Rp>(RpcCall request, CancellationToken cancellation = default)
		{
			var cr = Post(request, cancellation);

			if(cr.StatusCode != System.Net.HttpStatusCode.OK)
				throw new ApiCallException(cr.StatusCode.ToString() + " " + cr.Content.ReadAsStringAsync().Result);

			try
			{
				return JsonSerializer.Deserialize<Rp>(cr.Content.ReadAsStringAsync().Result, Options);
			}
			catch(Exception ex)
			{
				throw new ApiCallException("Response deserialization failed", ex);
			}
		}
		
		public void SendOnly(RpcCall request, CancellationToken cancellation = default)
		{
			var cr = Post(request, cancellation);
			
			if(cr.StatusCode != System.Net.HttpStatusCode.OK)
				throw new ApiCallException(cr.StatusCode.ToString() + " " + cr.Content.ReadAsStringAsync().Result);
		}
	}
}
