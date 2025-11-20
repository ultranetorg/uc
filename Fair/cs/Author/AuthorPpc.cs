namespace Uccs.Fair;

public class AuthorPpc : FairPpc<AuthorPpr>
{
	public new AutoId	Id { get; set; }

	public AuthorPpc()
	{
	}

	public AuthorPpc(AutoId id)
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
			
			return new AuthorPpr {Author = e};
		}
	}
}

public class AuthorPpr : PeerResponse
{
	public Author	Author {get; set;}
}
