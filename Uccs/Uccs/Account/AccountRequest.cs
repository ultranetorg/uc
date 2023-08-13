namespace Uccs.Net
{
	public class AccountRequest : RdcRequest
	{
		public AccountAddress		Account {get; set;}

		public override RdcResponse Execute(Sun sun)
		{
 			lock(sun.Lock)
			{
	 			if(sun.Synchronization != Synchronization.Synchronized)
					throw new RdcNodeException(RdcNodeError.NotSynchronized);

				var ai = sun.Mcv.Accounts.Find(Account, sun.Mcv.LastConfirmedRound.Id);

				if(ai == null)
					throw new RdcEntityException(RdcEntityError.AccountNotFound);

 				return new AccountResponse{Account = ai};
			}
		}
	}
	
	public class AccountResponse : RdcResponse
	{
		public AccountEntry Account {get; set;}
	}
}
