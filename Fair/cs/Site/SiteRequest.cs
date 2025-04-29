namespace Uccs.Fair;

public class SiteRequest : FairPpc<SiteResponse>
{
	public new AutoId	Id { get; set; }

	public SiteRequest()
	{
	}

	public SiteRequest(AutoId id)
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
			
			return new SiteResponse {Site = e};
		}
	}
}

public class SiteResponse : PeerResponse
{
	public Site	Site {get; set;}
}
