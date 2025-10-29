using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Uccs.Fair;

public abstract class FairApc : McvApc
{
	public abstract object Execute(FairNode node, HttpListenerRequest request, HttpListenerResponse response, Flow flow);

	public override object Execute(McvNode mcv, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		return Execute(mcv as FairNode, request, response, flow);
	}
}

public class FairTypeResolver : ApiTypeResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var ti = base.GetTypeInfo(type, options);

        if(ti.Type == typeof(FuncPeerRequest))
			foreach(var i in typeof(FairPpcClass).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(FuncPeerRequest)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Remove(i.Name.Length - "Request".Length))))
				ti.PolymorphismOptions.DerivedTypes.Add(i);

        else if(ti.Type == typeof(PeerResponse))
			foreach(var i in typeof(FairPpcClass).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(PeerResponse)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Remove(i.Name.Length - "Response".Length))))
				ti.PolymorphismOptions.DerivedTypes.Add(i);

        else if(ti.Type == typeof(CodeException))
			foreach(var i in typeof(ProductException).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(CodeException)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Remove(i.Name.Length - "Exception".Length))))
				ti.PolymorphismOptions.DerivedTypes.Add(i);

        else if(ti.Type == typeof(SiteOperation))
 		{
            ti.PolymorphismOptions = new JsonPolymorphismOptions
									 {
										TypeDiscriminatorPropertyName = "$type",
										IgnoreUnrecognizedTypeDiscriminators = true,
										UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
									 };
			foreach(var i in typeof(SiteOperation).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(SiteOperation)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name)))
 				ti.PolymorphismOptions.DerivedTypes.Add(i);
		}

        else if(ti.Type == typeof(VotableOperation))
 		{
            ti.PolymorphismOptions = new JsonPolymorphismOptions
									 {
										TypeDiscriminatorPropertyName = "$type",
										IgnoreUnrecognizedTypeDiscriminators = true,
										UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
									 };

			foreach(var i in typeof(VotableOperation).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(VotableOperation)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name)))
 				ti.PolymorphismOptions.DerivedTypes.Add(i);
		}

        else if(ti.Type == typeof(Operation))
 			foreach(var i in typeof(FairOperation).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(FairOperation)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name)))
 				ti.PolymorphismOptions.DerivedTypes.Add(i);

        return ti;
    }
}

public class FairApiServer : McvApiServer
{
	FairNode Node;

	public FairApiServer(FairNode node, ApiSettings settings, Flow flow) : base(node, settings, flow, FairApiClient.CreateOptions())
	{
		Node = node;
	}

	protected override Type Create(string call)
	{
		return Type.GetType(typeof(FairApc).Namespace + '.' + call) ?? base.Create(call);
	}

	protected override object Execute(object call, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		if(call is FairApc a)
			return a.Execute(Node, request, response, flow);
		else
			return base.Execute(call, request, response, flow);
	}
}

public class FairApiClient : McvApiClient
{
	new public static JsonSerializerOptions CreateOptions()
	{
		var o = McvApiClient.CreateOptions();

		o.TypeInfoResolver = new FairTypeResolver();
		
		return o;
	}

	public FairApiClient(string address, string accesskey, HttpClient http = null, int timeout = 30) : base(address, accesskey, http, timeout)
	{
		Options = CreateOptions();
	}
}

public class CostApc : FairApc
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

	public override object Execute(FairNode rdn, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		if(Rate == 0)
		{
			Rate = 1;
		}

		var r = rdn.Peering.Call(() => new CostRequest(), flow);

		return new Return {	//RentBytePerDay				= r.RentPerBytePerDay * Rate,
							//Exeunit						= r.ConsensusExeunitFee * Rate,
			
							//RentAccount					= FairOperation.ToBD(Mcv.EntityLength, Mcv.Forever) * Rate,
				
							//RentAuthor					= Years.Select(y => PublisherLengths.Select(l => FairOperation.NameFee(y, new string(' ', l)) * Rate).ToArray()).ToArray(),
				
							//RentProduct					= Years.Select(y => FairOperation.ToBD(Mcv.EntityLength, Time.FromYears(y)) * Rate).ToArray(),
			
							//RentProductData				= Years.Select(y => FairOperation.ToBD(1, Time.FromYears(y)) * Rate).ToArray(),
							//RentProductDataForever		= FairOperation.SpacetimeFee(1, Mcv.Forever) * Rate
							};
	}
}

public class SearchResult
{
	public string		Text { get; set; }
	public AutoId		Entity { get; set; }

	public override string ToString()
	{
		return $"{Text}, {Entity}";
	}
}

public class PublicationsSearchApc : FairApc
{
	public AutoId		Site { get; set; }
	public string		Query { get; set; }
	public int			Skip { get; set; }
	public int			Take { get; set; } = 10;

	public override object Execute(FairNode node, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(node.Mcv.Lock)
			return node.Mcv.Publications.Search(Site, Query, Skip, Take);
	}
}