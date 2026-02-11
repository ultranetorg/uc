namespace Uccs.Net;

public class ExamineTransactionPpc : McvPpc<ExamineTransactionPpr>
{
	public Transaction Transaction { get; set; }

	public override Result Execute()
	{
		lock(Peering.Lock)
			lock(Mcv.Lock)
			{
				var u = Mcv.Users.Latest(Transaction.User);

				Transaction.Expiration = Mcv.LastConfirmedRound.Id + 1;

				if(Peering.ValidateIncoming(Transaction, false, out var r))
				{
					var b = r.AffectedUsers.Values.First(i => u == null ? i.Name == Transaction.User : i.Id == u.Id);
				
					var atr = new ExamineTransactionPpr
							  {
								  LastConfirmedRid	= Mcv.LastConfirmedRound.Id,
								  NextNid			= Transaction.Nonce
							  };

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

public class ExamineTransactionPpr : Result
{
	public int			LastConfirmedRid { get; set; }
	public int			NextNid { get; set; }
	public long			SpacetimeConsumed { get; set; }
	public long			EnergyConsumed { get; set; }
	//public AutoId		Generator { get; set; }
}
