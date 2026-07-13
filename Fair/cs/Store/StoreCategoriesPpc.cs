namespace Uccs.Fair;

public class StoreCategoriesPpc : FairPpc<StoreCategoriesPpr>
{
	public AutoId		Store {get; set;}

	public StoreCategoriesPpc()
	{
	}

	public StoreCategoriesPpc(AutoId siteid)
	{
		Store = siteid;
	}

	public override Result Execute()
	{
		RequireGraph();

		var e = Mcv.Stores.Latest(Store);
			
		if(e == null)
			throw new EntityException(EntityError.NotFound);
			
		return new StoreCategoriesPpr {Categories = e.Categories};
	}
}

public class StoreCategoriesPpr : Result
{
	public AutoId[] Categories {get; set;}
}
