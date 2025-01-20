namespace Uccs.Smp;

public class AccountAuthorsRequest : McvPpc<AccountAuthorsResponse>
{
	public AccountIdentifier		Identifier {get; set;}

	public AccountAuthorsRequest()
	{
	}

	public AccountAuthorsRequest(AccountIdentifier identifier)
	{
		Identifier = identifier;
	}

	public AccountAuthorsRequest(AccountAddress addres)
	{
		Identifier = new(addres);
	}

	public AccountAuthorsRequest(EntityId id)
	{
		Identifier = new(id);
	}

	public override PeerResponse Execute()
	{
 		lock(Mcv.Lock)
		{
			RequireBase();

			SmpAccountEntry e;

			if(Identifier.Address != null)
				e = Mcv.Accounts.Find(Identifier.Address, Mcv.LastConfirmedRound.Id) as SmpAccountEntry;
			else if(Identifier.Id != null)
				e = Mcv.Accounts.Find(Identifier.Id, Mcv.LastConfirmedRound.Id) as SmpAccountEntry;
			else
				throw new RequestException(RequestError.IncorrectRequest);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new AccountAuthorsResponse {Authors = e.Authors};
		}
	}
}

public class AccountAuthorsResponse : PeerResponse
{
	public EntityId[] Authors {get; set;}
}
