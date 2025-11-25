namespace Uccs.Rdn;

public class QueryResourcePpc : RdnPpc<QueryResourcePpr>
{
	public string		Query { get; set; }

	public override Return Execute()
	{
 			lock(Mcv.Lock)
		{	
			return new QueryResourcePpr {Resources = Mcv.SearchResources(Query).Select(i => i.Address).ToArray()};
		}
	}
}
	
public class QueryResourcePpr : Return
{
	public Ura[] Resources { get; set; }
}
