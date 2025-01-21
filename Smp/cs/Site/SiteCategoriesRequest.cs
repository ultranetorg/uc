namespace Uccs.Smp;

public class SiteCategoriesRequest : SmpPpc<SiteCategoriesResponse>
{
	public EntityId		Site {get; set;}

	public SiteCategoriesRequest()
	{
	}

	public SiteCategoriesRequest(EntityId siteid)
	{
		Site = siteid;
	}

	public override PeerResponse Execute()
	{
 		lock(Mcv.Lock)
		{
			RequireBase();

			var e = Mcv.Sites.Find(Site, Mcv.LastConfirmedRound.Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new SiteCategoriesResponse {Categories = e.Categories};
		}
	}
}

public class SiteCategoriesResponse : PeerResponse
{
	public EntityId[] Categories {get; set;}
}
