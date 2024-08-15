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
				Transaction.EUFee	= a?.ECBalance ?? 0;

				var r = Mcv.TryExecute(Transaction);
				
				if(Transaction.Successful)
				{
					var b = r.AffectedAccounts[Transaction.Signer];
					
					return new AllocateTransactionResponse {Generetor			= Mcv.Accounts.Find(m.Account, Mcv.LastConfirmedRound.Id).Id,
															LastConfirmedRid	= Mcv.LastConfirmedRound.Id,
															PowHash				= Mcv.LastConfirmedRound.Hash,
															NextNid				= Transaction.Nid,
															STCost				= a.BYBalance - b.BYBalance,
															EUCostMinimum		= a.BandwidthExpiration > Mcv.LastConfirmedRound.ConsensusTime ? 0 : Transaction.EUSpent};
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
		public Unit			STCost { get; set; }
		public Unit			EUCostMinimum { get; set; }
		public EntityId		Generetor { get; set; }
	}
}
