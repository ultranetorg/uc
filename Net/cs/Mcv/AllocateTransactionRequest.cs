namespace Uccs.Net
{
	public class AllocateTransactionRequest : McvCall<AllocateTransactionResponse>
	{
		public Transaction Transaction {get; set;}

		public override PeerResponse Execute()
		{
			lock(Mcv.Lock)
			{
				RequireMember();

				var a = Mcv.Accounts.Find(Transaction.Signer, Mcv.LastConfirmedRound.Id);
				
				if(!Transaction.EmissionOnly && a == null)
					throw new EntityException(EntityError.NotFound);

				Transaction.Nid = a?.LastTransactionNid + 1 ?? 0;
				Transaction.Fee = Immission.End;

				Mcv.TryExecute(Transaction);
				
				var m = Mcv.NextVoteMembers.NearestBy(m => m.Account, Transaction.Signer).Account;

				if(Transaction.Successful)
				{
					return new AllocateTransactionResponse {Generetor			= Mcv.Accounts.Find(m, Mcv.LastConfirmedRound.Id).Id,
															LastConfirmedRid	= Mcv.LastConfirmedRound.Id,
															PowHash				= Mcv.LastConfirmedRound.Hash,
															NextNid				= Transaction.Nid,
															MinFee				= Transaction.Operations.SumMoney(i => i.ExeUnits * Mcv.LastConfirmedRound.ConsensusExeunitFee),
															};
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
		public Money		MinFee { get; set; }
		public EntityId		Generetor { get; set; }
	}
}
