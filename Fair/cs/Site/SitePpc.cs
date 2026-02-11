namespace Uccs.Fair;

public class SitePpc : FairPpc<SitePpr>
{
	public AutoId	Id { get; set; }

	public SitePpc()
	{
	}

	public SitePpc(AutoId id)
	{
		Id = id;
	}

	public override Result Execute()
	{
		if(Id == null)
			throw new RequestException(RequestError.IncorrectRequest);

 		lock(Mcv.Lock)
		{	
			RequireGraph();

			var	e = Mcv.Sites.Latest(Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new SitePpr {Site = e};
		}
	}
}

public class SitePpr : Result
{
	public Site	Site {get; set;}
}
