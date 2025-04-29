namespace Uccs.Fair;

public class FairAccountRequest : McvPpc<FairAccountResponse>
{
	public AccountIdentifier		Identifier {get; set;}

	public FairAccountRequest()
	{
	}

	public FairAccountRequest(AccountIdentifier identifier)
	{
		Identifier = identifier;
	}

	public FairAccountRequest(AccountAddress addres)
	{
		Identifier = new(addres);
	}

	public FairAccountRequest(AutoId id)
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
			
			return new FairAccountResponse {Account = e};
		}
	}
}

public class FairAccountResponse : PeerResponse
{
	public FairAccount Account {get; set;}
}
