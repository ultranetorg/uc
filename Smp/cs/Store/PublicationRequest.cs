namespace Uccs.Smp;

public class PublicationRequest : SmpPpc<PublicationResponse>
{
	public new EntityId	Id { get; set; }

	public PublicationRequest()
	{
	}

	public PublicationRequest(EntityId id)
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

			var	e = Mcv.Publications.Find(Id, Mcv.LastConfirmedRound.Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new PublicationResponse {Publication = e};
		}
	}
}

public class PublicationResponse : PeerResponse
{
	public Publication	Publication {get; set;}
}
