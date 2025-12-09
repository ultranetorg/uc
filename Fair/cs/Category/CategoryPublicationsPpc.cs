namespace Uccs.Fair;

public class CategoryPublicationsPpc : FairPpc<CategoryPublicationsPpr>
{
	public AutoId		Category { get; set; }

	public CategoryPublicationsPpc()
	{
	}

	public CategoryPublicationsPpc(AutoId id)
	{
		Category = id;
	}

	public override Result Execute()
	{
 		lock(Mcv.Lock)
		{
			RequireGraph();

			var e = Mcv.Categories.Latest(Category);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new CategoryPublicationsPpr {Publications = e.Publications};
		}
	}
}

public class CategoryPublicationsPpr : Result
{
	public AutoId[] Publications { get; set; }
}
