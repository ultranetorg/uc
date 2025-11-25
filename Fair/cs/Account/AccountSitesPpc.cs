namespace Uccs.Fair;

public class AccountSitesPpc : McvPpc<AccountSitesPpr>
{
	public AccountIdentifier		Identifier {get; set;}

	public AccountSitesPpc()
	{
	}

	public AccountSitesPpc(AccountIdentifier identifier)
	{
		Identifier = identifier;
	}

	public AccountSitesPpc(AccountAddress addres)
	{
		Identifier = new(addres);
	}

	public AccountSitesPpc(AutoId id)
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
			
			return new AccountSitesPpr {Sites = e.ModeratedSites};
		}
	}
}

public class AccountSitesPpr : Return
{
	public AutoId[] Sites {get; set;}
}
