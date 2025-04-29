namespace Uccs.Fair;

public class ReviewRequest : FairPpc<ReviewResponse>
{
	public new AutoId	Id { get; set; }

	public ReviewRequest()
	{
	}

	public ReviewRequest(AutoId id)
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
			
			return new ReviewResponse {Review = e};
		}
	}
}

public class ReviewResponse : PeerResponse
{
	public Review	Review {get; set;}
}
