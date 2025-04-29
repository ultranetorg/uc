namespace Uccs.Fair;

public class PublicationRequest : FairPpc<PublicationResponse>
{
	public new AutoId	Id { get; set; }

	public PublicationRequest()
	{
	}

	public PublicationRequest(AutoId id)
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
