namespace Uccs.Fair;

public class ProposalCommentPpc : FairPpc<ProposalCommentPpr>
{
	public new AutoId	Id { get; set; }

	public ProposalCommentPpc()
	{
	}

	public ProposalCommentPpc(AutoId id)
	{
		Id = id;
	}

	public override Return Execute()
	{
		if(Id == null)
			throw new RequestException(RequestError.IncorrectRequest);

 		lock(Mcv.Lock)
		{	
			RequireGraph();

			var	e = Mcv.ProposalComments.Find(Id, Mcv.LastConfirmedRound.Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new ProposalCommentPpr {Comment = e};
		}
	}
}

public class ProposalCommentPpr : Return
{
	public ProposalComment	Comment {get; set;}
}
