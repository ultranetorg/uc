namespace Uccs.Fair;

public class StorePpc : FairPpc<StorePpr>
{
	public AutoId	Id { get; set; }

	public StorePpc()
	{
	}

	public StorePpc(AutoId id)
	{
		Id = id;
	}

	public override Result Execute()
	{
		if(Id == null)
			throw new RequestException(RequestError.IncorrectRequest);

		RequireGraph();

		var	e = Mcv.Stores.Latest(Id);
			
		if(e == null)
			throw new EntityException(EntityError.NotFound);
			
		return new StorePpr {Store = e};
	}
}

public class StorePpr : Result
{
	public Store	Store {get; set;}
}
