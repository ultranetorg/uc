namespace Uccs.Net
{
	public class AllocateTransactionRequest : RdcCall<AllocateTransactionResponse>
	{
		public Transaction			Transaction {get; set;}

		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
			{
				RequireMember(sun);

				var a = sun.Mcv.Accounts.Find(Transaction.Signer, sun.Mcv.LastConfirmedRound.Id);
				
				if(a == null)
					throw new EntityException(EntityError.NotFound);

				Transaction.Nid = a.LastTransactionNid + 1;
				Transaction.Fee = Emission.End;

				sun.TryExecute(Transaction);
				
				if(Transaction.Successful)
				{
					return new AllocateTransactionResponse {LastConfirmedRid	= sun.Mcv.LastConfirmedRound.Id,
															PowHash				= sun.Mcv.LastConfirmedRound.Hash,
															NextTransactionId	= a == null ? 0 : (a.LastTransactionNid + 1),
															MinFee				= Transaction.Operations.SumMoney(i => i.Fee)};
				}				
				else
					throw new EntityException(EntityError.ExcutionFailed);
			}
		}
	}
	
	public class AllocateTransactionResponse : RdcResponse
	{
		public int		LastConfirmedRid { get; set; }
		public int		NextTransactionId { get; set; }
		public byte[]	PowHash { get; set; }
		public Money	MinFee { get; set; }
	}
}
