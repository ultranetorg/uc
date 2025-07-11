namespace Uccs.Fair;

public class ProposalCommentRequest : FairPpc<ProposalCommentResponse>
{
	public new AutoId	Id { get; set; }

	public ProposalCommentRequest()
	{
	}

	public ProposalCommentRequest(AutoId id)
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

			var	e = Mcv.ProposalComments.Find(Id, Mcv.LastConfirmedRound.Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new ProposalCommentResponse {Comment = e};
		}
	}
}

public class ProposalCommentResponse : PeerResponse
{
	public ProposalComment	Comment {get; set;}
}
