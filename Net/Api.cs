using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Uccs.Net;

public abstract class Api
{
	public const string						Nexus = "v0/nexus";
	public const string						Vault = "v0/vault";

	public static ushort					MapPort(Zone zone) => (ushort)(zone + (ushort)KnownProtocol.Api);
	public static string					ForSystem(Zone zone, IPAddress ip, string path, bool ssl = false) => $@"http{(ssl ? "s" : "")}://{ip}:{MapPort(zone)}/{path}";
	public static string					ForNode(Net net, IPAddress ip, bool ssl = false) => $@"http{(ssl ? "s" : "")}://{ip}:{MapPort(net.Zone)}/node/v0/{net.Name}";
}

public class NetJsonConfiguration : JsonConfiguration
{
	public NetJsonConfiguration()
	{
		Options = CreateOptions();
	}

	new public static JsonSerializerOptions CreateOptions()
	{
		var o = JsonConfiguration.CreateOptions();

		o.Converters.Add(new UnitJsonConverter());
		o.Converters.Add(new AccountJsonConverter());
		o.Converters.Add(new AccountKeyJsonConverter());
		o.Converters.Add(new AutoIdJsonConverter());
		o.Converters.Add(new TimeJsonConverter());
		o.Converters.Add(new EntityAddressJsonConverter());

		o.TypeInfoResolver = new ApiTypeResolver();

		return o;
	}
}

public class IpApiSettings : Settings
{
	public IPAddress		LocalIP { get; set; } = Net.DefaultHost;
	public IPAddress		PublicIP { get; set; }
	public string			PublicAccessKey { get; set; }
	public bool				Ssl { get; set; }

	public IpApiSettings() : base(XonTextValueSerializator.Default)
	{
	}

	public string PublicNodeAddress(Net net) 
	{
		return Api.ForNode(net, PublicIP, Ssl);
	}

	public string LocalNodeAddress(Net net)
	{
		return Api.ForNode(net, LocalIP, false);
	}

	public string PublicSystemAddress(Zone zone, string path) 
	{
		return Api.ForSystem(zone, PublicIP, path, Ssl);
	}

	public string LocalSystemAddress(Zone zone, string path)
	{
		return Api.ForSystem(zone, LocalIP, path, false);
	}

	public ApiSettings ToSystemSettings(Zone zone, string path)
	{
		return	new ApiSettings
				{
					LocalAddress = LocalSystemAddress(zone, path),
					PublicAddress = PublicIP == null ? null : PublicSystemAddress(zone, path),
					PublicAccessKey = PublicAccessKey,
				};
	}

	public ApiSettings ToNodeSettings(Net net)
	{
		return	new ApiSettings
				{
					LocalAddress = LocalNodeAddress(net),
					PublicAddress = PublicIP == null ? null : PublicNodeAddress(net),
					PublicAccessKey = PublicAccessKey,
				};
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

        if(ti.Type == typeof(PeerRequest))
        {
            ti.PolymorphismOptions = new JsonPolymorphismOptions
									 {
										TypeDiscriminatorPropertyName = "$type",
										IgnoreUnrecognizedTypeDiscriminators = true,
										UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
									 };

			foreach(var i in typeof(Net).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(ti.Type) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Substring(0, i.Name.Length - "Ppc".Length))))
			{
				ti.PolymorphismOptions.DerivedTypes.Add(i);
			}
        }

        if(ti.Type == typeof(Result))
        {
            ti.PolymorphismOptions = new JsonPolymorphismOptions
									 {
										TypeDiscriminatorPropertyName = "$type",
										IgnoreUnrecognizedTypeDiscriminators = true,
										UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
									 };

			foreach(var i in typeof(Net).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(ti.Type) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Substring(0, i.Name.Length - "Ppr".Length))))
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

			foreach(var i in typeof(Net).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(ti.Type) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Substring(0, i.Name.Length - "Exception".Length))))
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

			foreach(var i in typeof(Net).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(ti.Type) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name)))
			{
				ti.PolymorphismOptions.DerivedTypes.Add(i);
			}
        }

        return ti;
    }
}

public class JsonApiClient : JsonClient
{
	public JsonApiClient(string address, string accesskey, HttpClient http = null, int timeout = 30) : base(address, accesskey, http, timeout)
	{
		Options = NetJsonConfiguration.CreateOptions();
	}
}
