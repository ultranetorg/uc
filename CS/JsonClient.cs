using System.Diagnostics;
using System.Net;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs;

public class ApiCallException : Exception
{
	public HttpResponseMessage Response;

	public ApiCallException(HttpResponseMessage response, string msg) : base(msg)
	{ 
		Response = response;
	}

	public ApiCallException(string msg, Exception ex) : base(msg, ex){ }
	public ApiCallException(string msg) : base(msg){ }
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

public class JsonClient
{
	HttpClient						Http;
	public string					Address;
	string							Key;
	public int						Failures;
	public JsonSerializerOptions	Options;

	public JsonClient(string address, string accesskey, HttpClient http = null, int timeout = 30)
	{
		Http = http;
		Address = address;
		Key = accesskey;

		if(http == null)
		{
			Http = new HttpClient();
			Http.Timeout = Debugger.IsAttached ? Timeout.InfiniteTimeSpan : TimeSpan.FromSeconds(timeout);
		} 
		else
		{
			Http = http;
		}
	}

	public override string ToString()
	{
		return Address;
	}

	public HttpResponseMessage Send(Apc request, Flow flow)
	{
		var c = JsonSerializer.Serialize(request, request.GetType(), Options);

		using(var m = new HttpRequestMessage(HttpMethod.Get, $"{Address}/{Apc.NameOf(request.GetType())}" + (Key == null ? null : $"?{Apc.AccessKey}={Key}")))
		{
			m.Content = new StringContent(c, Encoding.UTF8, "application/json");

			try
			{
				var rp = Http.Send(m, flow.Cancellation);

				if(rp.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
					throw JsonSerializer.Deserialize<CodeException>(rp.Content.ReadAsStringAsync().Result, Options);

				if(rp.StatusCode != System.Net.HttpStatusCode.OK)
					throw new ApiCallException(rp, rp.Content.ReadAsStringAsync().Result);

				return rp;
			}
			catch(HttpRequestException ex)
			{
				throw new ApiCallException(ex.Message, ex);
			}
		}
	}

	public async Task<HttpResponseMessage> SendAsync(Apc request, Flow workflow)
	{
		var c = JsonSerializer.Serialize(request, request.GetType(), Options);

		using(var m = new HttpRequestMessage(HttpMethod.Get, $"{Address}/{Apc.NameOf(request.GetType())}?{Apc.AccessKey}={Key}"))
		{
			m.Content = new StringContent(c, Encoding.UTF8, "application/json");

			var cr = await Http.SendAsync(m, workflow.Cancellation);

			if(cr.StatusCode != System.Net.HttpStatusCode.OK)
				throw new ApiCallException(cr, cr.Content.ReadAsStringAsync().Result);

			return cr;
		}
	}

	public Rp Request<Rp>(Apc request, Flow flow)
	{
		using(var cr = Send(request, flow))
		{
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

	public async Task<Rp> RequestAsync<Rp>(Apc request, Flow flow)
	{
		using(var cr = await SendAsync(request, flow))
		{
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
