namespace Uccs.Rdn;

public class QueryResourcePpc : RdnPpc<QueryResourcePpr>
{
	public string		Query { get; set; }

	public override PeerResponse Execute()
	{
 			lock(Mcv.Lock)
		{	
			return new QueryResourcePpr {Resources = Mcv.SearchResources(Query).Select(i => i.Address).ToArray()};
		}
	}
}
	
public class QueryResourcePpr : PeerResponse
{
	public Ura[] Resources { get; set; }
}
