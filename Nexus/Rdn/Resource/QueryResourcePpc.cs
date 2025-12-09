namespace Uccs.Rdn;

public class QueryResourcePpc : RdnPpc<QueryResourcePpr>
{
	public string		Query { get; set; }

	public override Result Execute()
	{
 			lock(Mcv.Lock)
		{	
			return new QueryResourcePpr {Resources = Mcv.SearchResources(Query).Select(i => i.Address).ToArray()};
		}
	}
}
	
public class QueryResourcePpr : Result
{
	public Ura[] Resources { get; set; }
}
