namespace Uccs.Fair;

public class CategoryCategoriesRequest : FairPpc<CategoryCategoriesResponse>
{
	public AutoId		Category {get; set;}

	public CategoryCategoriesRequest()
	{
	}

	public CategoryCategoriesRequest(AutoId id)
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
			
			return new CategoryCategoriesResponse {Categories = e.Categories};
		}
	}
}

public class CategoryCategoriesResponse : PeerResponse
{
	public AutoId[] Categories {get; set;}
}
