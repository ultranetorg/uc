namespace Uccs.Fair;

public class CategoryRequest : FairPpc<CategoryResponse>
{
	public new EntityId	Id { get; set; }

	public CategoryRequest()
	{
	}

	public CategoryRequest(EntityId id)
	{
		Id = id;
	}

	public override PeerResponse Execute()
	{
		if(Id == null)
			throw new RequestException(RequestError.IncorrectRequest);

 		lock(Mcv.Lock)
		{	
			RequireBase();

			var	e = Mcv.Categories.Find(Id, Mcv.LastConfirmedRound.Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new CategoryResponse {Category = e};
		}
	}
}

public class CategoryResponse : PeerResponse
{
	public Category	Category {get; set;}
}
