namespace Uccs.Fair;

public class SitePpc : FairPpc<SitePpr>
{
	public new AutoId	Id { get; set; }

	public SitePpc()
	{
	}

	public SitePpc(AutoId id)
	{
		Id = id;
	}

	public override PeerResponse Execute()
	{
		if(Id == null)
			throw new RequestException(RequestError.IncorrectRequest);

 		lock(Mcv.Lock)
		{	
			RequireGraph();

			var	e = Mcv.Sites.Find(Id, Mcv.LastConfirmedRound.Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new SitePpr {Site = e};
		}
	}
}

public class SitePpr : PeerResponse
{
	public Site	Site {get; set;}
}
