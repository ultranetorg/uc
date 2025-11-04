namespace Uccs.Rdn;

public class QueryResourceRequest : RdnPpc<QueryResourceResponse>
{
	public string		Query { get; set; }

	public override PeerResponse Execute()
	{
 			lock(Mcv.Lock)
		{	
			return new QueryResourceResponse {Resources = Mcv.SearchResources(Query).Select(i => i.Address).ToArray()};
		}
	}
}
	
public class QueryResourceResponse : PeerResponse
{
	public Ura[] Resources { get; set; }
}
