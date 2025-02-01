namespace Uccs.Smp;

public class ReviewRequest : SmpPpc<ReviewResponse>
{
	public new EntityId	Id { get; set; }

	public ReviewRequest()
	{
	}

	public ReviewRequest(EntityId id)
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
