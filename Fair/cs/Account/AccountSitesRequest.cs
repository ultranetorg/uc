﻿namespace Uccs.Fair;

public class AccountSitesRequest : McvPpc<AccountSitesResponse>
{
	public AccountIdentifier		Identifier {get; set;}

	public AccountSitesRequest()
	{
	}

	public AccountSitesRequest(AccountIdentifier identifier)
	{
		Identifier = identifier;
	}

	public AccountSitesRequest(AccountAddress addres)
	{
		Identifier = new(addres);
	}

	public AccountSitesRequest(AutoId id)
	{
		Identifier = new(id);
	}

	public override PeerResponse Execute()
	{
 		lock(Mcv.Lock)
		{
			RequireGraph();

			FairAccount e;

			if(Identifier.Address != null)
				e = Mcv.Accounts.Find(Identifier.Address, Mcv.LastConfirmedRound.Id) as FairAccount;
			else if(Identifier.Id != null)
				e = Mcv.Accounts.Find(Identifier.Id, Mcv.LastConfirmedRound.Id) as FairAccount;
			else
				throw new RequestException(RequestError.IncorrectRequest);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new AccountSitesResponse {Sites = e.ModeratedSites};
		}
	}
}

public class AccountSitesResponse : PeerResponse
{
	public AutoId[] Sites {get; set;}
}
