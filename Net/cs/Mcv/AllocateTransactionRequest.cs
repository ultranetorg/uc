namespace Uccs.Net;

public class AllocateTransactionRequest : McvPpc<AllocateTransactionResponse>
{
	public Transaction Transaction { get; set; }

	public override PeerResponse Execute()
	{
		lock(Mcv.Lock)
		{
			var m = RequireMemberFor(Transaction.Signer);

			var a = Mcv.Accounts.Find(Transaction.Signer, Mcv.LastConfirmedRound.Id);

			if(a == null)
				throw new EntityException(EntityError.NotFound);
			
#if IMMISSION
			if(!Transaction.EmissionOnly && a == null)
				throw new EntityException(EntityError.NotFound);
#endif
			Transaction.Nid		= a.LastTransactionNid + 1;
			Transaction.ECFee	= a.Integrate(Mcv.LastConfirmedRound.ConsensusTime);

			var r = Mcv.TryExecute(Transaction);
			
			if(Transaction.Successful)
			{
				var b = r.AffectedAccounts[Transaction.Signer];
				
				return new AllocateTransactionResponse {Generator			= m.Id,
														LastConfirmedRid	= Mcv.LastConfirmedRound.Id,
														PowHash				= Mcv.LastConfirmedRound.Hash,
														NextNid				= Transaction.Nid,
														BYCost				= a.BYBalance - b.BYBalance,
														ECCost				= a.BandwidthExpiration > Mcv.LastConfirmedRound.ConsensusTime ? 0 : Transaction.ECSpent};
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
	public long			BYCost { get; set; }
	public long			ECCost { get; set; }
	public EntityId		Generator { get; set; }
}
