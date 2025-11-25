namespace Uccs.Net;

public class AccountPpc : McvPpc<AccountPpr>
{
	public AccountIdentifier		Identifier {get; set;}

	public AccountPpc()
	{
	}

	public AccountPpc(AccountIdentifier identifier)
	{
		Identifier = identifier;
	}

	public AccountPpc(AccountAddress addres)
	{
		Identifier = new(addres);
	}

	public AccountPpc(AutoId id)
	{
		Identifier = new(id);
	}

	public override Return Execute()
	{
 		lock(Mcv.Lock)
		{
			RequireGraph();

			Account e;

			if(Identifier.Address != null)
				e = Mcv.Accounts.Find(Identifier.Address, Mcv.LastConfirmedRound.Id);
			else if(Identifier.Id != null)
				e = Mcv.Accounts.Find(Identifier.Id, Mcv.LastConfirmedRound.Id);
			else
				throw new RequestException(RequestError.IncorrectRequest);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new AccountPpr {Account = e};
		}
	}
}

public class AccountPpr : Return
{
	public Account Account {get; set;}
}
