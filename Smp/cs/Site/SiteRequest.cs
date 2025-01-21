namespace Uccs.Smp;

public class SiteRequest : SmpPpc<SiteResponse>
{
	public new EntityId	Id { get; set; }

	public SiteRequest()
	{
	}

	public SiteRequest(EntityId id)
	{
		Id = id;
	}

	public override PeerResponse Execute()
	{
		if(Id == null)
			throw new RequestException(RequestError.IncorrectRequest);

 		lock(Mcv.Lock)
		{	
			RequireBase();

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
