﻿namespace Uccs.Net
{
	public class AccountRequest : McvCall<AccountResponse>
	{
		public AccountIdentifier		Identifier {get; set;}

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

		public override PeerResponse Execute()
		{
 			lock(Node.Lock)
			{
				RequireBase();

				Account e;

				if(Identifier.Address != null)
					e = Mcv.Accounts.Find(Identifier.Address, Mcv.LastConfirmedRound.Id);
				else if(Identifier.Id != null)
					e = Mcv.Accounts.Find(Identifier.Id, Mcv.LastConfirmedRound.Id);
				else
					throw new RequestException(RequestError.IncorrectRequest);
				
				if(e == null)
					throw new EntityException(EntityError.NotFound);
				
				return new AccountResponse {Account = e};
			}
		}
	}
	
	public class AccountResponse : PeerResponse
	{
		public Account Account {get; set;}
	}
}