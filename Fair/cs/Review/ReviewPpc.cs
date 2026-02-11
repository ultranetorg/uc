namespace Uccs.Fair;

public class ReviewPpc : FairPpc<ReviewPpr>
{
	public AutoId	Id { get; set; }

	public ReviewPpc()
	{
	}

	public ReviewPpc(AutoId id)
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

			var	e = Mcv.Reviews.Latest(Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new ReviewPpr {Review = e};
		}
	}
}

public class ReviewPpr : Result
{
	public Review	Review {get; set;}
}
