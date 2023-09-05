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

namespace Uccs.Net
{
	public class ApiCallException : Exception
	{
		public ApiCallException(string msg) : base(msg){ }
		public ApiCallException(string msg, Exception ex) : base(msg, ex){ }
	}

	public class IPJsonConverter : JsonConverter<IPAddress>
	{
		public override IPAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return IPAddress.Parse(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
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

			Options.IgnoreReadOnlyProperties = false;

			Options.Converters.Add(new CoinJsonConverter());
			Options.Converters.Add(new AccountJsonConverter());
			Options.Converters.Add(new IPJsonConverter());
			Options.Converters.Add(new ChainTimeJsonConverter());
			Options.Converters.Add(new ReleaseAddressJsonConverter());
			Options.Converters.Add(new VersionJsonConverter());
			Options.Converters.Add(new XonDocumentJsonConverter());
		}

		public JsonClient(HttpClient http, string server, Zone zone, string accesskey)
		{
			HttpClient = http;
			Address = $"http://{server}:{zone.JsonPort}";
			Key = accesskey;
		}

		HttpResponseMessage Send(ApiCall request, Workflow workflow) 
		{
			request.ProtocolVersion = Sun.Versions.First().ToString();
			request.AccessKey = Key;

			var c = JsonSerializer.Serialize(request, request.GetType(), Options);

			using(var m = new HttpRequestMessage(HttpMethod.Get, Address + "/" + ApiCall.NameOf(request.GetType())))
			{
				m.Content = new StringContent(c, Encoding.UTF8, "application/json");
	
				return HttpClient.Send(m, workflow.Cancellation);
			}
		}

		public Rp Request<Rp>(ApiCall request, Workflow workflow)
		{
			using(var cr = Send(request, workflow))
			{
				if(cr.StatusCode != System.Net.HttpStatusCode.OK)
					throw new ApiCallException(cr.StatusCode.ToString() + " " + cr.Content.ReadAsStringAsync().Result);

				try
				{
					return JsonSerializer.Deserialize<Rp>(cr.Content.ReadAsStringAsync().Result, Options);
				}
				catch(Exception ex)
				{
					throw new ApiCallException(ex.ToString());
				}
			}
		}
		
		public void Post(ApiCall request, Workflow workflow)
		{
			var cr = Send(request, workflow);
			
			if(cr.StatusCode != System.Net.HttpStatusCode.OK)
				throw new ApiCallException(cr.StatusCode.ToString() + " " + cr.Content.ReadAsStringAsync().Result);
		}

		public SettingsResponse GetSettings(Workflow workflow)
		{
			return Request<SettingsResponse>(new SettingsCall {}, workflow);
		}

		public void InstallPackage(PackageAddress release, Workflow workflow)
		{
			Post(new InstallPackageCall {Release = release}, workflow);
		}

		public ReleaseStatus GetReleaseStatus(PackageAddress release, Workflow workflow)
		{
			return Request<ReleaseStatus>(new PackageStatusCall {Release = release}, workflow);
		}

	}
}
