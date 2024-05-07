namespace Uccs.Net
{
	public class AccountRequest : RdcCall<AccountResponse>
	{
		public AccountIdentifier Identifier {get; set;}
		
		public AccountRequest()
		{
		}

		public AccountRequest(AccountIdentifier identifier)
		{
			Identifier = identifier;
		}

		public AccountRequest(AccountAddress addres)
		{
			Identifier = new(addres);
		}

		public AccountRequest(EntityId id)
		{
			Identifier = new(id);
		}

		public override RdcResponse Execute(Sun sun)
		{
 			lock(sun.Lock)
			{
				RequireBase(sun);

				Account e;

				if(Identifier.Address != null)
					e = sun.Mcv.Accounts.Find(Identifier.Address, sun.Mcv.LastConfirmedRound.Id);
				else if(Identifier.Id != null)
					e = sun.Mcv.Accounts.Find(Identifier.Id, sun.Mcv.LastConfirmedRound.Id);
				else
					throw new RequestException(RequestError.IncorrectRequest);
				
				if(e == null)
					throw new EntityException(EntityError.NotFound);
				
				return new AccountResponse {Account = e};
			}
		}
	}
	
	public class AccountResponse : RdcResponse
	{
		public Account Account {get; set;}
	}
}
