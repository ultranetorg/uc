namespace Uccs.Fair;

public class AuthorRequest : FairPpc<AuthorResponse>
{
	public new AutoId	Id { get; set; }

	public AuthorRequest()
	{
	}

	public AuthorRequest(AutoId id)
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

			var	e = Mcv.Authors.Find(Id, Mcv.LastConfirmedRound.Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new AuthorResponse {Author = e};
		}
	}
}

public class AuthorResponse : PeerResponse
{
	public Author	Author {get; set;}
}
