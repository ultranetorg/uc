namespace Uccs.Fair;

public class SiteCategoriesRequest : FairPpc<SiteCategoriesResponse>
{
	public AutoId		Site {get; set;}

	public SiteCategoriesRequest()
	{
	}

	public SiteCategoriesRequest(AutoId siteid)
	{
		Site = siteid;
	}

	public override PeerResponse Execute()
	{
 		lock(Mcv.Lock)
		{
			RequireGraph();

			var e = Mcv.Sites.Find(Site, Mcv.LastConfirmedRound.Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new SiteCategoriesResponse {Categories = e.Categories};
		}
	}
}

public class SiteCategoriesResponse : PeerResponse
{
	public AutoId[] Categories {get; set;}
}
