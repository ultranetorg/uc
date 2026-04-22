using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Uccs.Rdn;

namespace Uccs.Nexus;

public class NexusJsonConfiguration : NetJsonConfiguration
{
	new public static JsonSerializerOptions CreateOptions()
	{
		var o = NetJsonConfiguration.CreateOptions();

		o.TypeInfoResolver = new NexusTypeResolver();

		o.Converters.Add(new UraJsonConverter());
		o.Converters.Add(new UrrJsonConverter());
		o.Converters.Add(new ResourceDataJsonConverter());

		return o;
	}
}

public class NexusTypeResolver : ApiTypeResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var ti = base.GetTypeInfo(type, options);

        if(ti.Type == typeof(PackageActivityProgress))
		{
            ti.PolymorphismOptions = new JsonPolymorphismOptions
									 {
										TypeDiscriminatorPropertyName = "$type",
										IgnoreUnrecognizedTypeDiscriminators = true,
										UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
									 };

			foreach(var i in typeof(PackageActivityProgress).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(ti.Type) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name)))
				ti.PolymorphismOptions.DerivedTypes.Add(i);
		}

        return ti;
    }
}

public class NexusApiClient : JsonApiClient
{
	public PackageInfo FindLocalPackage(Ura address, Flow flow) => Call<PackageInfo>(new LocalPackageApc { Address = address }, flow);

	public NexusApiClient(string address, string accesskey, HttpClient http = null, int timeout = 30) : base(address, accesskey, http, timeout)
	{
		Options = NexusJsonConfiguration.CreateOptions();
	}

	public PackageInfo DeployPackage(Ura address, string desination, Flow flow)
	{
		Send(new PackageDeployApc { Address = address, DeploymentPath = desination }, flow);

		do
		{
			var d = Call<PackageActivityProgress>(new PackageActivityProgressApc { Package = address }, flow);

			if(d is null)
			{
				return Call<PackageInfo>(new LocalPackageApc { Address = address }, flow);

				//if(lrr.Availability == Availability.Full)
				//{
				//	return lrr;
				//}
				//else
				//{
				//	throw new ResourceException(ResourceError.);
				//}
			}

			Thread.Sleep(100);
		}
		while(flow.Active);

		throw new OperationCanceledException();
	}
}

public class NexusApiServer : JsonServer
{
	Nexus Nexus;

	public NexusApiServer(Nexus nexus, Flow flow) : base(nexus.Settings.Api.ToSystemSettings(nexus.Settings.Zone, Api.Nexus), NexusJsonConfiguration.CreateOptions(), flow)
	{
		Nexus = nexus;
	}

	protected override Type Create(string call)
	{
		return Type.GetType(typeof(NexusApiServer).Namespace + '.' + call) ?? Assembly.GetAssembly(typeof(NodeApc)).GetType(typeof(McvApc).Namespace + '.' + call);
	}

	protected override object Execute(object call, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		if(call is INexusApc u) 
			return u.Execute(Nexus, request, response, flow);

		throw new ApiCallException("Unknown call");
	}
}

public interface INexusApc
{
	public abstract object Execute(Nexus nexus, HttpListenerRequest request, HttpListenerResponse response, Flow flow);
}

public class NexusPropertyApc : Apc, INexusApc
{
	public string Path { get; set; }

	public object Execute(Nexus nexus, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		object o = nexus;

		foreach(var i in Path.Split('.'))
		{
			o = o.GetType().GetProperty(i)?.GetValue(o) ?? o.GetType().GetField(i)?.GetValue(o);

			if(o == null)
				throw new NodeException(NodeError.NotFound);
		}

		switch(o)
		{
			case byte[] b:
				return b.ToHex();

			default:
				return o?.ToString();
		}
	}
}

public class NexusOpenApc : Apc, INexusApc
{
	public Snp Request { get; set; }

	public object Execute(Nexus nexus, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		nexus.Start(Request, flow);

		return null;
	}
}

public class NnpNodeApc : Apc, INexusApc
{
	public string Net { get; set; }

	public object Execute(Nexus nexus, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(nexus)
			return nexus.NnpIppServer.Locals.Find(i => i.Net == Net);
	}
}

public class NnpCallApc : Apc, INexusApc
{
	public string			Net { get; set; }
	public Argumentation	Argumentation { get; set; }

	public object Execute(Nexus nexus, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(nexus)
			return nexus.NnpPeering.Call(Net, Argumentation, flow);
	}
}

//public class TransactNncApc : Apc, INexusApc
//{
//	public TransactNna	Argumentation { get; set; }
//
//	public object Execute(Nexus nexus, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
//	{
//		lock(nexus)
//			return nexus.NnpIppServer.Relay(null, Argumentation);
//	}
//}
//