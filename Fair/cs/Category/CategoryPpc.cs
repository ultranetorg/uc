namespace Uccs.Fair;

public class CategoryPpc : FairPpc<CategoryPpr>
{
	public new AutoId	Id { get; set; }

	public CategoryPpc()
	{
	}

	public CategoryPpc(AutoId id)
	{
		Id = id;
	}

	public override Return Execute()
	{
		if(Id == null)
			throw new RequestException(RequestError.IncorrectRequest);

 		lock(Mcv.Lock)
		{	
			RequireGraph();

			var	e = Mcv.Categories.Find(Id, Mcv.LastConfirmedRound.Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new CategoryPpr {Category = e};
		}
	}
}

public class CategoryPpr : Return
{
	public Category	Category {get; set;}
}
