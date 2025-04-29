namespace Uccs.Fair;

public class CategoryPublicationsRequest : FairPpc<CategoryPublicationsResponse>
{
	public AutoId		Category {get; set;}

	public CategoryPublicationsRequest()
	{
	}

	public CategoryPublicationsRequest(AutoId id)
	{
		Category = id;
	}

	public override PeerResponse Execute()
	{
 		lock(Mcv.Lock)
		{
			RequireGraph();

			var e = Mcv.Categories.Find(Category, Mcv.LastConfirmedRound.Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new CategoryPublicationsResponse {Publications = e.Publications};
		}
	}
}

public class CategoryPublicationsResponse : PeerResponse
{
	public AutoId[] Publications {get; set;}
}
