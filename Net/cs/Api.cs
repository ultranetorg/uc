using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Uccs.Net;

public class IpApiSettings : Settings
{
	public IPAddress		LocalIP { get; set; } = Net.DefaultHost;
	public IPAddress		PublicIP { get; set; }
	public string			PublicAccessKey { get; set; }
	public bool				Ssl { get; set; }

	public string			PublicAddress(Net net) => $@"http{(Ssl ? "s" : "")}://{PublicIP}:{Net.MapPort(net, KnownSystem.Api)}";
	public string			LocalAddress(Net net) => $@"http://{LocalIP}:{Net.MapPort(net, KnownSystem.Api)}";

	public ApiSettings		ToApiSettings(Net net) =>	new ApiSettings
														{
															LocalAddress = LocalAddress(net),
															PublicAddress = PublicIP == null ? null : PublicAddress(net),
															PublicAccessKey = PublicAccessKey,
														};

	public string			PublicAddress(Zone zone, KnownSystem system) => $@"http{(Ssl ? "s" : "")}://{PublicIP}:{Net.MapPort(zone, system)}";
	public string			LocalAddress(Zone zone, KnownSystem system) => $@"http://{LocalIP}:{Net.MapPort(zone, system)}";

	public ApiSettings		ToApiSettings(Zone zone, KnownSystem system) =>	new ApiSettings
																			{
																				LocalAddress = LocalAddress(zone, system),
																				PublicAddress = PublicIP == null ? null : PublicAddress(zone, system),
																				PublicAccessKey = PublicAccessKey,
																			};

	public IpApiSettings() : base(XonTextValueSerializator.Default)
	{
	}
}


public class ApiTypeResolver : DefaultJsonTypeInfoResolver
{
	public ApiTypeResolver()
	{
		Modifiers.Add(	ti =>
						{
							if (ti.Type.IsSubclassOf(typeof(CodeException)))
							{
								foreach(var i in ti.Properties.Where(i => i.Name != nameof(CodeException.ErrorCode)))
								{
									i.ShouldSerialize = (p, v) => false;
								}
							}
						});
	}

	public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var ti = base.GetTypeInfo(type, options);

        if(ti.Type == typeof(FuncPeerRequest))
        {
            ti.PolymorphismOptions = new JsonPolymorphismOptions
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
            ti.PolymorphismOptions = new JsonPolymorphismOptions
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

        if(ti.Type == typeof(CodeException))
        {
            ti.PolymorphismOptions = new JsonPolymorphismOptions
									 {
										TypeDiscriminatorPropertyName = "$type",
										IgnoreUnrecognizedTypeDiscriminators = true,
										UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
									 };

			foreach(var i in typeof(Net).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(CodeException)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Remove(i.Name.Length - "Exception".Length))))
			{
				ti.PolymorphismOptions.DerivedTypes.Add(i);
			}
        }

        if(ti.Type == typeof(Operation))
        {
            ti.PolymorphismOptions = new JsonPolymorphismOptions
									 {
										TypeDiscriminatorPropertyName = "$type",
										IgnoreUnrecognizedTypeDiscriminators = true,
										UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
									 };

			foreach(var i in typeof(Net).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(Operation)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name)))
			{
				ti.PolymorphismOptions.DerivedTypes.Add(i);
			}
        }

        return ti;
    }
}

public class ApiClient : JsonClient
{
	public static string	GetAddress(Zone zone, IPAddress ip, bool ssl, KnownSystem system) => $@"http{(ssl ? "s" : "")}://{ip}:{Net.MapPort(zone, system)}";

	public static JsonSerializerOptions CreateOptions()
	{
		var o = new JsonSerializerOptions {};
		
		o.IgnoreReadOnlyProperties = true;

		o.Converters.Add(new UnitJsonConverter());
		o.Converters.Add(new AccountJsonConverter());
		o.Converters.Add(new EntityIdJsonConverter());
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
