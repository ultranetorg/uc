using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Uccs.Net;

public class ApiTypeResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var ti = base.GetTypeInfo(type, options);

        if(ti.Type == typeof(FuncPeerRequest))
        {
            ti.PolymorphismOptions =	new JsonPolymorphismOptions
										{
											TypeDiscriminatorPropertyName = "$type",
											IgnoreUnrecognizedTypeDiscriminators = true,
											UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
										};

			foreach(var i in typeof(Net).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(FuncPeerRequest)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Remove(i.Name.Length - "Request".Length))))
			{
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

			foreach(var i in typeof(Net).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(PeerResponse)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Remove(i.Name.Length - "Response".Length))))
			{
				ti.PolymorphismOptions.DerivedTypes.Add(i);
			}
        }

        if(ti.Type == typeof(NetException))
        {
            ti.PolymorphismOptions =	new JsonPolymorphismOptions
										{
											TypeDiscriminatorPropertyName = "$type",
											IgnoreUnrecognizedTypeDiscriminators = true,
											UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
										};

			foreach(var i in typeof(Net).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(NetException)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Remove(i.Name.Length - "Exception".Length))))
			{
				ti.PolymorphismOptions.DerivedTypes.Add(i);
			}
        }

        return ti;
    }
}

public class ApiClient : JsonClient
{
	public static JsonSerializerOptions CreateOptions()
	{
		var o = new JsonSerializerOptions{};
		
		o.IgnoreReadOnlyProperties = true;
		
		o.Converters.Add(new UnitJsonConverter());
		o.Converters.Add(new AccountJsonConverter());
		o.Converters.Add(new IPJsonConverter());
		o.Converters.Add(new TimeJsonConverter());
		o.Converters.Add(new VersionJsonConverter());
		o.Converters.Add(new XonJsonConverter());
		o.Converters.Add(new BigIntegerJsonConverter());

#if ETHEREUM
		DefaultOptions.Converters.Add(new HexBigIntegerJsonConverter());
#endif

		o.TypeInfoResolver = new ApiTypeResolver();

		return o;
	}


	public ApiClient(HttpClient http, string address, string accesskey) : base(http, address, accesskey)
	{
		Options = CreateOptions();
	}

	public ApiClient(string address, string accesskey, int timeout = 30) : base(address, accesskey, timeout)
	{
		Options = CreateOptions();
	}
}
