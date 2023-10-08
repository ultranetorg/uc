namespace Uccs.Net
{
	public class AccountRequest : RdcRequest
	{
		public AccountAddress		Account {get; set;}

		public override RdcResponse Execute(Sun sun)
		{
 			lock(sun.Lock)
			{
	 			RequireSynchronizedBase(sun);

				var ai = sun.Mcv.Accounts.Find(Account, sun.Mcv.LastConfirmedRound.Id);

				if(ai == null)
					throw new RdcEntityException(RdcEntityError.NotFound);

 				return new AccountResponse{Account = ai};
			}
		}
	}
	
	public class AccountResponse : RdcResponse
	{
		public Account Account {get; set;}
	}
}
