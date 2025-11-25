namespace Uccs.Fair;

public class CategoryCategoriesPpc : FairPpc<CategoryCategoriesPPr>
{
	public AutoId		Category {get; set;}

	public CategoryCategoriesPpc()
	{
	}

	public CategoryCategoriesPpc(AutoId id)
	{
		Category = id;
	}

	public override Return Execute()
	{
 		lock(Mcv.Lock)
		{
			RequireGraph();

			var e = Mcv.Categories.Find(Category, Mcv.LastConfirmedRound.Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new CategoryCategoriesPPr {Categories = e.Categories};
		}
	}
}

public class CategoryCategoriesPPr : Return
{
	public AutoId[] Categories {get; set;}
}
