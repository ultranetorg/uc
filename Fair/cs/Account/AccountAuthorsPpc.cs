namespace Uccs.Fair;

public class AccountAuthorsPpc : McvPpc<AccountAuthorsPpr>
{
	public AccountIdentifier		Identifier {get; set;}

	public AccountAuthorsPpc()
	{
	}

	public AccountAuthorsPpc(AccountIdentifier identifier)
	{
		Identifier = identifier;
	}

	public AccountAuthorsPpc(AccountAddress addres)
	{
		Identifier = new(addres);
	}

	public AccountAuthorsPpc(AutoId id)
	{
		Identifier = new(id);
	}

	public override Return Execute()
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
			
			return new AccountAuthorsPpr {Authors = e.Authors};
		}
	}
}

public class AccountAuthorsPpr : Return
{
	public AutoId[] Authors {get; set;}
}
