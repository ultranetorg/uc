namespace Uccs.Smp;

public class CategoryCategoriesRequest : SmpPpc<CategoryCategoriesResponse>
{
	public EntityId		Category {get; set;}

	public CategoryCategoriesRequest()
	{
	}

	public CategoryCategoriesRequest(EntityId id)
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
			
			return new CategoryCategoriesResponse {Categories = e.Categories};
		}
	}
}

public class CategoryCategoriesResponse : PeerResponse
{
	public EntityId[] Categories {get; set;}
}
