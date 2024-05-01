namespace Uccs.Net
{
	public class AllocateTransactionRequest : RdcCall<AllocateTransactionResponse>
	{
		public Transaction Transaction {get; set;}

		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
			{
				RequireMember(sun);

				var a = sun.Mcv.Accounts.Find(Transaction.Signer, sun.Mcv.LastConfirmedRound.Id);
				
				if(!Transaction.EmissionOnly && a == null)
					throw new EntityException(EntityError.NotFound);

				Transaction.Nid = a?.LastTransactionNid + 1 ?? 0;
				Transaction.Fee = Emission.End;

				sun.TryExecute(Transaction);
				
				var m = sun.NextVoteMembers.NearestBy(m => m.Account, Transaction.Signer).Account;

				if(Transaction.Successful)
				{
					return new AllocateTransactionResponse {Generetor			= sun.Mcv.Accounts.Find(m, sun.Mcv.LastConfirmedRound.Id).Id,
															LastConfirmedRid	= sun.Mcv.LastConfirmedRound.Id,
															PowHash				= sun.Mcv.LastConfirmedRound.Hash,
															NextNid				= Transaction.Nid,
															MinFee				= Transaction.Operations.SumMoney(i => i.ExeUnits * sun.Mcv.LastConfirmedRound.ConsensusExeunitFee),
															};
				}				
				else
					throw new EntityException(EntityError.ExcutionFailed);
			}
		}
	}
	
	public class AllocateTransactionResponse : RdcResponse
	{
		public int			LastConfirmedRid { get; set; }
		public int			NextNid { get; set; }
		public byte[]		PowHash { get; set; }
		public Money		MinFee { get; set; }
		public EntityId		Generetor { get; set; }
	}
}
