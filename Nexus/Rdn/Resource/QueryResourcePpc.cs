namespace Uccs.Rdn;

public class QueryResourcePpc : RdnPpc<QueryResourcePpr>
{
	public AutoId		Domain { get; set; }
	public string		Query { get; set; }

	public override Result Execute()
	{
 		lock(Mcv.Lock)
		{	
			return new QueryResourcePpr {Resources = Mcv.SearchResources(Domain, Query).ToArray()};
		}
	}
}
	
public class QueryResourcePpr : Result
{
	public Resource[] Resources { get; set; }
}
