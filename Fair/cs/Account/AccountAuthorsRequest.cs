﻿namespace Uccs.Fair;

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

	public AccountAuthorsRequest(AutoId id)
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
			
			return new AccountAuthorsResponse {Authors = e.Authors};
		}
	}
}

public class AccountAuthorsResponse : PeerResponse
{
	public AutoId[] Authors {get; set;}
}
