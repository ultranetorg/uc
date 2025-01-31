using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Uccs.Smp;

public abstract class SmpApc : McvApc
{
	public abstract object Execute(FairNode node, HttpListenerRequest request, HttpListenerResponse response, Flow workflow);

	public override object Execute(McvNode mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		return Execute(mcv as FairNode, request, response, workflow);
	}
}

public class SmpTypeResolver : ApiTypeResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var ti = base.GetTypeInfo(type, options);

        if(ti.Type == typeof(FuncPeerRequest))
        {
			foreach(var i in typeof(SmpPpcClass).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(FuncPeerRequest)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Remove(i.Name.Length - "Request".Length))))
			{
				ti.PolymorphismOptions.DerivedTypes.Add(i);
			}
        }

        if(ti.Type == typeof(PeerResponse))
        {
			foreach(var i in typeof(SmpPpcClass).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(PeerResponse)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Remove(i.Name.Length - "Response".Length))))
			{
				ti.PolymorphismOptions.DerivedTypes.Add(i);
			}
        }

        if(ti.Type == typeof(NetException))
        {
			foreach(var i in typeof(ProductException).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(NetException)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Remove(i.Name.Length - "Exception".Length))))
			{
				ti.PolymorphismOptions.DerivedTypes.Add(i);
			}
        }

        return ti;
    }
}

public class SmpApiServer : McvApiServer
{
	FairNode Node;

	public SmpApiServer(FairNode node, ApiSettings settings, Flow workflow) : base(node, settings, workflow, SmpApiClient.CreateOptions(node.Net))
	{
		Node = node;
	}

	protected override Type Create(string call)
	{
		return Type.GetType(typeof(SmpApc).Namespace + '.' + call) ?? base.Create(call);
	}

	protected override object Execute(object call, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		if(call is SmpApc a)
			return a.Execute(Node, request, response, flow);
		else
			return base.Execute(call, request, response, flow);
	}
}

public class SmpApiClient : McvApiClient
{
	new public static JsonSerializerOptions CreateOptions(Net.Net net)
	{
		var o = McvApiClient.CreateOptions(net);

		o.TypeInfoResolver = new SmpTypeResolver();
		
		return o;
	}

	public SmpApiClient(HttpClient http, McvNet net, string address, string accesskey) : base(http, net, address, accesskey)
	{
		Options = CreateOptions(net);
		Net = net;
		
	}

	public SmpApiClient(McvNet net, string address, string accesskey, int timeout = 30) : base(net, address, accesskey, timeout)
	{
		Options = CreateOptions(net);
		Net = net;
	}
}

public class CostApc : SmpApc
{
	public class Return
	{
		//public Money		RentBytePerDay { get; set; }
		//public Money		Exeunit { get; set; }

		public Unit		RentAccount { get; set; }
		public Unit[][]	RentAuthor { get; set; }
		public Unit[]	RentProduct { get; set; }
		public Unit[]	RentProductData { get; set; }
		public Unit		RentProductDataForever { get; set; }
	}

	public Unit		Rate { get; set; } = 1;
	public byte[]	Years { get; set; }

	public override object Execute(FairNode rdn, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		if(Rate == 0)
		{
			Rate = 1;
		}

		var r = rdn.Peering.Call(() => new CostRequest(), workflow);

		return new Return {	//RentBytePerDay				= r.RentPerBytePerDay * Rate,
							//Exeunit						= r.ConsensusExeunitFee * Rate,
			
							RentAccount					= SmpOperation.SpacetimeFee(Mcv.EntityLength, Mcv.Forever) * Rate,
				
							//RentAuthor					= Years.Select(y => PublisherLengths.Select(l => SmpOperation.NameFee(y, new string(' ', l)) * Rate).ToArray()).ToArray(),
				
							RentProduct					= Years.Select(y => SmpOperation.SpacetimeFee(Mcv.EntityLength, Time.FromYears(y)) * Rate).ToArray(),
			
							RentProductData				= Years.Select(y => SmpOperation.SpacetimeFee(1, Time.FromYears(y)) * Rate).ToArray(),
							RentProductDataForever		= SmpOperation.SpacetimeFee(1, Mcv.Forever) * Rate};
	}
}
