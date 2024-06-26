using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Nethereum.Hex.HexTypes;

namespace Uccs.Net
{
	public class ApiTypeResolver : DefaultJsonTypeInfoResolver
	{
	    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
	    {
	        var ti = base.GetTypeInfo(type, options);

	        if(ti.Type == typeof(PeerRequest))
	        {
	            ti.PolymorphismOptions =	new JsonPolymorphismOptions
											{
												TypeDiscriminatorPropertyName = "$type",
												IgnoreUnrecognizedTypeDiscriminators = true,
												UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
											};

				foreach(var i in typeof(PeerCallClass).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(PeerRequest)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Remove(i.Name.Length - "Request".Length))))
				{
					if(i.DerivedType == null)
						throw new IntegrityException();

					ti.PolymorphismOptions.DerivedTypes.Add(i);
				}
	        }

	        if(ti.Type == typeof(PeerResponse))
	        {
	            ti.PolymorphismOptions =	new JsonPolymorphismOptions
											{
												TypeDiscriminatorPropertyName = "$type",
												IgnoreUnrecognizedTypeDiscriminators = true,
												UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
											};

				foreach(var i in typeof(PeerCallClass).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(PeerResponse)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Remove(i.Name.Length - "Response".Length))))
				{
					if(i.DerivedType == null)
						throw new IntegrityException();

					ti.PolymorphismOptions.DerivedTypes.Add(i);
				}
	        }
	
	        return ti;
	    }
	}

	public class ApiClient : JsonClient
	{
		public static readonly JsonSerializerOptions DefaultOptions;

		static ApiClient()
		{
			DefaultOptions = new JsonSerializerOptions{};

			DefaultOptions.IgnoreReadOnlyProperties = true;

			DefaultOptions.Converters.Add(new CoinJsonConverter());
			DefaultOptions.Converters.Add(new AccountJsonConverter());
			DefaultOptions.Converters.Add(new IPJsonConverter());
			DefaultOptions.Converters.Add(new TimeJsonConverter());
			DefaultOptions.Converters.Add(new VersionJsonConverter());
			DefaultOptions.Converters.Add(new XonDocumentJsonConverter());
			DefaultOptions.Converters.Add(new BigIntegerJsonConverter());
			DefaultOptions.Converters.Add(new HexBigIntegerJsonConverter());

			DefaultOptions.TypeInfoResolver = new ApiTypeResolver();
		}

		public ApiClient(HttpClient http, string address, string accesskey) : base(http, address, accesskey, DefaultOptions)
		{
		}

		public ApiClient(string address, string accesskey, int timeout = 30) : base(address, accesskey, DefaultOptions, timeout)
		{
		}
	}

	public class HexBigIntegerJsonConverter : JsonConverter<HexBigInteger>
	{
		public override HexBigInteger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return new HexBigInteger(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, HexBigInteger value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.HexValue);
		}
	}
}
