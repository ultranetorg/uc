using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Uccs.Fair;

public abstract class FairApc : McvApc
{
	public abstract object Execute(FairNode node, HttpListenerRequest request, HttpListenerResponse response, Flow workflow);

	public override object Execute(McvNode mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		return Execute(mcv as FairNode, request, response, workflow);
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

        if(ti.Type == typeof(PeerResponse))
			foreach(var i in typeof(FairPpcClass).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(PeerResponse)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Remove(i.Name.Length - "Response".Length))))
				ti.PolymorphismOptions.DerivedTypes.Add(i);

        if(ti.Type == typeof(NetException))
			foreach(var i in typeof(ProductException).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(NetException)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Remove(i.Name.Length - "Exception".Length))))
				ti.PolymorphismOptions.DerivedTypes.Add(i);

         if(ti.Type == typeof(Operation))
 			foreach(var i in typeof(FairOperation).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(FairOperation)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name)))
 				ti.PolymorphismOptions.DerivedTypes.Add(i);

        return ti;
    }
}

public class FairApiServer : McvApiServer
{
	FairNode Node;

	public FairApiServer(FairNode node, ApiSettings settings, Flow workflow) : base(node, settings, workflow, FairApiClient.CreateOptions(node.Net))
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
	new public static JsonSerializerOptions CreateOptions(Net.Net net)
	{
		var o = McvApiClient.CreateOptions(net);

		o.TypeInfoResolver = new FairTypeResolver();
		
		return o;
	}

	public FairApiClient(HttpClient http, McvNet net, string address, string accesskey) : base(http, net, address, accesskey)
	{
		Options = CreateOptions(net);
		Net = net;
		
	}

	public FairApiClient(McvNet net, string address, string accesskey, int timeout = 30) : base(net, address, accesskey, timeout)
	{
		Options = CreateOptions(net);
		Net = net;
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

	public override object Execute(FairNode rdn, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		if(Rate == 0)
		{
			Rate = 1;
		}

		var r = rdn.Peering.Call(() => new CostRequest(), workflow);

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
	public EntityId		Entity { get; set; }

	public override string ToString()
	{
		return $"{Text}, {Entity}";
	}
}

public class PublicationsSearchApc : FairApc
{
	public EntityId		Site { get; set; }
	public string		Query { get; set; }
	public int			Skip { get; set; }
	public int			Take { get; set; } = 10;

	public override object Execute(FairNode node, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		lock(node.Mcv.Lock)
		{
			var result = node.Mcv.PublicationTitles.Search(Query, Take, i => i.References.ContainsKey(Site));

			return result.Select(i =>	{
											return new SearchResult {Entity = i.References[Site], Text = i.Text};
																								 
										}).ToArray();

// 			IEnumerable<TermSearchResult> result = null;
// 
// 			foreach(var w in Query.ToLowerInvariant().Split(' '))
//  		{
// 				IEnumerable<TermSearchResult> r = null;
// 
// 				r = node.Mcv.PublicationTitles.Search(Site, w, 10, Skip, Take, null)//.GroupBy(i => i.Entity).Select(i => new TermSearchResult {Distance = (byte)i.Sum(j => j.Distance), Entity = i.First().Entity});
// 																					.Select(i => new TermSearchResult {Distance = i.Distance, Entity = i.Entity});
// 				if(result == null)
// 					result = r;
// 				else
// 				//	result = result.Intersect(r, EqualityComparer<TermSearchResult>.Create((a, b) => a.Entity == b.Entity, i => i.Entity.GetHashCode()));
// 					result = result.Union(r, EqualityComparer<TermSearchResult>.Create((a, b) => a.Entity == b.Entity, i => i.Entity.GetHashCode()));
//  		}
// 
// 			return result.OrderBy(i => i.Distance)
// 						.Select(i =>	{
// 											var p = node.Mcv.Publications.Latest(i.Entity);
// 											var r = node.Mcv.Products.Latest(p.Product);
// 																								 
// 											var t = r.GetString(p.Fields.First(f => f.Name == ProductField.Title));
// 																								 
// 											return new TermSearchResult {Entity = i.Entity, Text = t, Distance = i.Distance};
// 																								 
// 										}).ToArray();
		}
	}
}