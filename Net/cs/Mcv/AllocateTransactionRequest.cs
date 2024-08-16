namespace Uccs.Net
{
	public class AllocateTransactionRequest : McvCall<AllocateTransactionResponse>
	{
		public Transaction Transaction {get; set;}

		public override PeerResponse Execute()
		{
			lock(Mcv.Lock)
			{
				var m = RequireMemberFor(Transaction.Signer);

				var a = Mcv.Accounts.Find(Transaction.Signer, Mcv.LastConfirmedRound.Id);
				
#if IMMISSION
				if(!Transaction.EmissionOnly && a == null)
					throw new EntityException(EntityError.NotFound);
#endif
				Transaction.Nid		= a?.LastTransactionNid + 1 ?? 0;
				Transaction.ECFee	= a?.ECBalance ?? 0;

				var r = Mcv.TryExecute(Transaction);
				
				if(Transaction.Successful)
				{
					var b = r.AffectedAccounts[Transaction.Signer];
					
					return new AllocateTransactionResponse {Generetor			= Mcv.Accounts.Find(m.Account, Mcv.LastConfirmedRound.Id).Id,
															LastConfirmedRid	= Mcv.LastConfirmedRound.Id,
															PowHash				= Mcv.LastConfirmedRound.Hash,
															NextNid				= Transaction.Nid,
															BYCost				= a.BYBalance - b.BYBalance,
															ECCostMinimum		= a.BandwidthExpiration > Mcv.LastConfirmedRound.ConsensusTime ? 0 : Transaction.ECSpent};
				}				
				else
					throw new EntityException(EntityError.ExcutionFailed);
			}
		}
	}
	
	public class AllocateTransactionResponse : PeerResponse
	{
		public int			LastConfirmedRid { get; set; }
		public int			NextNid { get; set; }
		public byte[]		PowHash { get; set; }
		public Unit			BYCost { get; set; }
		public Unit			ECCostMinimum { get; set; }
		public EntityId		Generetor { get; set; }
	}
}
