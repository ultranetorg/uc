namespace Uccs.Net
{
	public class AccountRequest : RdcRequest
	{
		public AccountAddress		Account {get; set;}

		public override RdcResponse Execute(Core core)
		{
 			lock(core.Lock)
			{
	 			if(core.Synchronization != Synchronization.Synchronized)
					throw new RdcNodeException(RdcNodeError.NotSynchronized);

				var ai = core.Database.Accounts.Find(Account, core.Database.LastConfirmedRound.Id);

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
