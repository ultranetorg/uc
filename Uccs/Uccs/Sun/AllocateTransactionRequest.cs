namespace Uccs.Net
{
	public class AllocateTransactionRequest : RdcRequest
	{
		public AccountAddress		Account {get; set;}

		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
			{
				RequireSynchronizedBase(sun);

				var a = sun.Mcv.Accounts.Find(Account, sun.Mcv.LastConfirmedRound.Id);
								
				return new AllocateTransactionResponse {LastConfirmedRid	= sun.Mcv.LastConfirmedRound.Id,
														PowHash				= sun.Mcv.LastConfirmedRound.Hash,
														NextTransactionId	= a == null ? 0 : a.LastTransactionNid + 1,
														ExeunitMinFee		= sun.Mcv.LastConfirmedRound.ConfirmedExeunitMinFee};
			}
		}
	}
	
	public class AllocateTransactionResponse : RdcResponse
	{
		public int		LastConfirmedRid { get; set; }
		public int		NextTransactionId { get; set; }
		public byte[]	PowHash { get; set; }
		public Money	ExeunitMinFee { get; set; }
	}

}
