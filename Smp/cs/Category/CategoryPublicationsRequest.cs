namespace Uccs.Smp;

public class CategoryPublicationsRequest : SmpPpc<CategoryPublicationsResponse>
{
	public EntityId		Category {get; set;}

	public CategoryPublicationsRequest()
	{
	}

	public CategoryPublicationsRequest(EntityId id)
	{
		Category = id;
	}

	public override PeerResponse Execute()
	{
 		lock(Mcv.Lock)
		{
			RequireBase();

			var e = Mcv.Categories.Find(Category, Mcv.LastConfirmedRound.Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new CategoryPublicationsResponse {Publications = e.Publications};
		}
	}
}

public class CategoryPublicationsResponse : PeerResponse
{
	public EntityId[] Publications {get; set;}
}
