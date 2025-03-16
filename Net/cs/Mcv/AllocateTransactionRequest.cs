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
			{	
				if(Transaction.Operations.All(i => i.NonExistingSignerAllowed))
					Transaction.Nid = 0;
				else
					throw new EntityException(EntityError.NotFound);
			}
			else
				Transaction.Nid	= a.LastTransactionNid + 1;

			var r = Mcv.TryExecute(Transaction);
			
			if(Transaction.Successful)
			{
				var b = r.AffectedAccounts.Values.First(i => i.Address == Transaction.Signer);
				
				var atr = new AllocateTransactionResponse  {Generator			= m.Id,
															LastConfirmedRid	= Mcv.LastConfirmedRound.Id,
															PowHash				= Mcv.LastConfirmedRound.Hash,
															NextNid				= Transaction.Nid};

				if(a != null)
				{
					atr.SpacetimeConsumed	= a.Spacetime - b.Spacetime;
					atr.EnergyConsumed		= a.BandwidthExpiration > Mcv.LastConfirmedRound.ConsensusTime.Days ? 0 : Transaction.EnergyConsumed;
				}
				else
				{
					atr.SpacetimeConsumed	= -b.Spacetime;
				}

				return atr;
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
	public long			SpacetimeConsumed { get; set; }
	public long			EnergyConsumed { get; set; }
	public EntityId		Generator { get; set; }
}
