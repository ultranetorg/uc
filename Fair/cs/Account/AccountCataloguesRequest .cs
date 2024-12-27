namespace Uccs.Fair;

public class AccountCataloguesRequest : McvPpc<AccountCataloguesResponse>
{
	public AccountIdentifier		Identifier {get; set;}

	public AccountCataloguesRequest()
	{
	}

	public AccountCataloguesRequest(AccountIdentifier identifier)
	{
		Identifier = identifier;
	}

	public AccountCataloguesRequest(AccountAddress addres)
	{
		Identifier = new(addres);
	}

	public AccountCataloguesRequest(EntityId id)
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
			
			return new AccountCataloguesResponse {Catalogues = e.Catalogues};
		}
	}
}

public class AccountCataloguesResponse : PeerResponse
{
	public EntityId[] Catalogues {get; set;}
}
