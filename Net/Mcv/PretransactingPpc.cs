namespace Uccs.Net;

public class PretransactingPpc : McvPpc<PretransactingPpr>
{
	public string User { get; set; }

	public override Result Execute()
	{
		//lock(Peering.Lock)
		{
			lock(Mcv.Lock)
			{
				var u = Mcv.Users.Latest(User);

				return  new PretransactingPpr
						{
							LastConfirmedRid	= Mcv.LastConfirmedRound.Id,
							NextNonce			= u?.LastNonce + 1 ?? 0
						};
			}


			//if(Peering.ValidateIncoming(Transaction, false, out var r))
			//{
			//	var b = r.AffectedUsers.Values.First(i => u == null ? i.Name == Transaction.User : i.Id == u.Id);
			//	
			//	var etr = new ExamineTransactionPpr
			//				{
			//					LastConfirmedRid	= Mcv.LastConfirmedRound.Id,
			//					NextNonce			= Transaction.Nonce
			//				};
			//
			//	if(u != null)
			//	{
			//		etr.SpacetimeConsumed	= u.Spacetime - b.Spacetime;
			//		etr.EnergyConsumed		= u.BandwidthExpiration > Mcv.LastConfirmedRound.ConsensusTime.Days ? 0 : Transaction.EnergyConsumed;
			//	}
			//	else
			//	{
			//		etr.SpacetimeConsumed	= -b.Spacetime;
			//	}
			//
			//	return etr;
			//}
			//else
			//	throw new EntityException(EntityError.ExcutionFailed);
		}
	}
}

public class PretransactingPpr : Result
{
	public int			LastConfirmedRid { get; set; }
	public int			NextNonce { get; set; }
	//public long			SpacetimeConsumed { get; set; }
	//public long			EnergyConsumed { get; set; }
	//public AutoId		Generator { get; set; }
}
