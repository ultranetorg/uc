namespace Uccs.Fair;

public class ReviewPpc : FairPpc<ReviewPpr>
{
	public new AutoId	Id { get; set; }

	public ReviewPpc()
	{
	}

	public ReviewPpc(AutoId id)
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

			var	e = Mcv.Reviews.Find(Id, Mcv.LastConfirmedRound.Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new ReviewPpr {Review = e};
		}
	}
}

public class ReviewPpr : PeerResponse
{
	public Review	Review {get; set;}
}
