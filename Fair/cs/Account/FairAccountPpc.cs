namespace Uccs.Fair;

public class FairAccountPpc : McvPpc<FairAccountPpr>
{
	public AccountIdentifier		Identifier {get; set;}

	public FairAccountPpc()
	{
	}

	public FairAccountPpc(AccountIdentifier identifier)
	{
		Identifier = identifier;
	}

	public FairAccountPpc(AccountAddress addres)
	{
		Identifier = new(addres);
	}

	public FairAccountPpc(AutoId id)
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
			
			return new FairAccountPpr {Account = e};
		}
	}
}

public class FairAccountPpr : Return
{
	public FairAccount Account {get; set;}
}
