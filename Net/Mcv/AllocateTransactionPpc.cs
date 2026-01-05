namespace Uccs.Net;

public class AllocateTransactionPpc : McvPpc<AllocateTransactionPpr>
{
	public Transaction Transaction { get; set; }

	public override Result Execute()
	{
		lock(Peering.Lock)
			lock(Mcv.Lock)
			{
				//var m = RequireMemberFor(Transaction.Signer);

				var u = Mcv.Users.Find(Transaction.User, Mcv.LastConfirmedRound.Id);

				Transaction.Expiration = Mcv.LastConfirmedRound.Id + 1;

				if(Peering.ValidateIncoming(Transaction, false, out var r))
				{
					var b = r.AffectedAccounts.Values.First(i => i.Name == Transaction.User);
				
					var atr = new AllocateTransactionPpr{//Generator			= m.Id,
														 LastConfirmedRid	= Mcv.LastConfirmedRound.Id,
														 NextNid			= Transaction.Nonce};

					if(u != null)
					{
						atr.SpacetimeConsumed	= u.Spacetime - b.Spacetime;
						atr.EnergyConsumed		= u.BandwidthExpiration > Mcv.LastConfirmedRound.ConsensusTime.Days ? 0 : Transaction.EnergyConsumed;
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

public class AllocateTransactionPpr : Result
{
	public int			LastConfirmedRid { get; set; }
	public int			NextNid { get; set; }
	public long			SpacetimeConsumed { get; set; }
	public long			EnergyConsumed { get; set; }
	//public AutoId		Generator { get; set; }
}
