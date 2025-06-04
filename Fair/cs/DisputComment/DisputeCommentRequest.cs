namespace Uccs.Fair;

public class DisputeCommentRequest : FairPpc<DisputeCommentResponse>
{
	public new AutoId	Id { get; set; }

	public DisputeCommentRequest()
	{
	}

	public DisputeCommentRequest(AutoId id)
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

			var	e = Mcv.DisputeComments.Find(Id, Mcv.LastConfirmedRound.Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new DisputeCommentResponse {DisputeComment = e};
		}
	}
}

public class DisputeCommentResponse : PeerResponse
{
	public DisputeComment	DisputeComment {get; set;}
}
