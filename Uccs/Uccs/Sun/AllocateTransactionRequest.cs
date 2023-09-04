namespace Uccs.Net
{
	public class AllocateTransactionRequest : RdcRequest
	{
		public AccountAddress		Account {get; set;}

		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
			{
				if(!sun.Settings.Roles.HasFlag(Role.Base))				throw new RdcNodeException(RdcNodeError.NotBase);
				if(sun.Synchronization != Synchronization.Synchronized)	throw new RdcNodeException(RdcNodeError.NotSynchronized);

				var a = sun.Mcv.Accounts.Find(Account, sun.Mcv.LastConfirmedRound.Id);
								
				return new AllocateTransactionResponse {MaxRoundId			= sun.Mcv.LastConfirmedRound.Id + Mcv.Pitch * 2,
														PowHash				= sun.Mcv.LastConfirmedRound.Hash,
														NextTransactionId	= a == null ? 0 : a.LastTransactionId + 1,
														PerByteMinFee		= sun.TransactionPerByteMinFee};
			}
		}
	}
	
	public class AllocateTransactionResponse : RdcResponse
	{
		public int		MaxRoundId { get; set; }
		public int		NextTransactionId { get; set; }
		public byte[]	PowHash { get; set; }
		public Coin		PerByteMinFee { get; set; }
	}

}
