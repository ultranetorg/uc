namespace Uccs.Smp;

public class AuthorRequest : SmpPpc<AuthorResponse>
{
	public new EntityId	Id { get; set; }

	public AuthorRequest()
	{
	}

	public AuthorRequest(EntityId id)
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
