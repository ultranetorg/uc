namespace Uccs.Fair;

public class PageRequest : FairPpc<PageResponse>
{
	public new EntityId	Id { get; set; }

	public PageRequest()
	{
	}

	public PageRequest(EntityId id)
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

			var	e = Mcv.Pages.Find(Id, Mcv.LastConfirmedRound.Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new PageResponse {Page = e};
		}
	}
}

public class PageResponse : PeerResponse
{
	public Page	Page {get; set;}
}
