namespace Uccs.Fair;

public class SiteCategoriesPpc : FairPpc<SiteCategoriesPpr>
{
	public AutoId		Site {get; set;}

	public SiteCategoriesPpc()
	{
	}

	public SiteCategoriesPpc(AutoId siteid)
	{
		Site = siteid;
	}

	public override Result Execute()
	{
 		lock(Mcv.Lock)
		{
			RequireGraph();

			var e = Mcv.Sites.Find(Site, Mcv.LastConfirmedRound.Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new SiteCategoriesPpr {Categories = e.Categories};
		}
	}
}

public class SiteCategoriesPpr : Result
{
	public AutoId[] Categories {get; set;}
}
