namespace Uccs.Fair;

public class FairAccountAuthorsRequest : McvPpc<FairAccountAuthorsResponse>
{
	public AccountIdentifier		Identifier {get; set;}

	public FairAccountAuthorsRequest()
	{
	}

	public FairAccountAuthorsRequest(AccountIdentifier identifier)
	{
		Identifier = identifier;
	}

	public FairAccountAuthorsRequest(AccountAddress addres)
	{
		Identifier = new(addres);
	}

	public FairAccountAuthorsRequest(EntityId id)
	{
		Identifier = new(id);
	}

	public override PeerResponse Execute()
	{
 		lock(Mcv.Lock)
		{
			RequireBase();

			FairAccountEntry e;

			if(Identifier.Address != null)
				e = Mcv.Accounts.Find(Identifier.Address, Mcv.LastConfirmedRound.Id) as FairAccountEntry;
			else if(Identifier.Id != null)
				e = Mcv.Accounts.Find(Identifier.Id, Mcv.LastConfirmedRound.Id) as FairAccountEntry;
			else
				throw new RequestException(RequestError.IncorrectRequest);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new FairAccountAuthorsResponse {Authors = e.Authors};
		}
	}
}

public class FairAccountAuthorsResponse : PeerResponse
{
	public EntityId[] Authors {get; set;}
}
