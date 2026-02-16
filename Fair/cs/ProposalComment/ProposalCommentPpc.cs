namespace Uccs.Fair;

public class ProposalCommentPpc : FairPpc<ProposalCommentPpr>
{
	public AutoId	Id { get; set; }

	public ProposalCommentPpc()
	{
	}

	public ProposalCommentPpc(AutoId id)
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

			var	e = Mcv.ProposalComments.Latest(Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new ProposalCommentPpr {Comment = e};
		}
	}
}

public class ProposalCommentPpr : Result
{
	public ProposalComment	Comment {get; set;}
}
