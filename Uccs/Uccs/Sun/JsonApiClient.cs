using System;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net
{
	public class ApiCallException : Exception
	{
		public HttpResponseMessage Response;

		public ApiCallException(HttpResponseMessage response, string msg) : base(msg)
		{ 
			Response = response;
		}
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

	public class BigIntegerJsonConverter : JsonConverter<BigInteger>
	{
		public override BigInteger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return BigInteger.Parse(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, BigInteger value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}

	public class JsonApiClient// : RpcClient
	{
		HttpClient			HttpClient;
		public string		Address;
		string				Key;
		public int			Failures;

		public static		JsonSerializerOptions Options;
		
		static JsonApiClient()
		{
			Options = new JsonSerializerOptions{};

			Options.IgnoreReadOnlyProperties = true;

			Options.Converters.Add(new CoinJsonConverter());
			Options.Converters.Add(new AccountJsonConverter());
			Options.Converters.Add(new IPJsonConverter());
			Options.Converters.Add(new ChainTimeJsonConverter());
			Options.Converters.Add(new ResourceAddressJsonConverter());
			Options.Converters.Add(new ReleaseAddressJsonConverter());
			Options.Converters.Add(new VersionJsonConverter());
			Options.Converters.Add(new XonDocumentJsonConverter());
			Options.Converters.Add(new OperationJsonConverter());
			Options.Converters.Add(new RdcRequestJsonConverter());
			Options.Converters.Add(new BigIntegerJsonConverter());
		}

		public JsonApiClient(HttpClient http, string address, string accesskey)
		{
			HttpClient = http;
			Address = address;
			Key = accesskey;
		}

		public HttpResponseMessage Send(ApiCall request, Workflow workflow)
		{
			var c = JsonSerializer.Serialize(request, request.GetType(), Options);

			using(var m = new HttpRequestMessage(HttpMethod.Get, $"{Address}/{ApiCall.NameOf(request.GetType())}?accesskey={Key}"))
			{
				m.Content = new StringContent(c, Encoding.UTF8, "application/json");
	
				var cr =  HttpClient.Send(m, workflow.Cancellation);

				if(cr.StatusCode != System.Net.HttpStatusCode.OK)
					throw new ApiCallException(cr, cr.Content.ReadAsStringAsync().Result);

				return cr;
			}
		}

		public Rp Request<Rp>(ApiCall request, Workflow workflow)
		{
			using(var cr = Send(request, workflow))
			{
				if(cr.StatusCode != System.Net.HttpStatusCode.OK)
					throw new ApiCallException(cr, cr.Content.ReadAsStringAsync().Result);

				try
				{
					return JsonSerializer.Deserialize<Rp>(cr.Content.ReadAsStringAsync().Result, Options);
				}
				catch(Exception ex)
				{
					throw new ApiCallException("Deserialization error", ex);
				}
			}
		}
	}
}
