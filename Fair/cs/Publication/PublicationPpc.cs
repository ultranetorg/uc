namespace Uccs.Fair;

public class PublicationPpc : FairPpc<PublicationPpr>
{
	public new AutoId	Id { get; set; }

	public PublicationPpc()
	{
	}

	public PublicationPpc(AutoId id)
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

			var	e = Mcv.Publications.Latest(Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new PublicationPpr {Publication = e};
		}
	}
}

public class PublicationPpr : Result
{
	public Publication	Publication {get; set;}
}
