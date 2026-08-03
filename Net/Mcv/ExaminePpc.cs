namespace Uccs.Net;

public class ExaminePpc : McvPpc<PretransactingPpr>
{
	public Operation[]	Operations { get; set; }
	public string		User { get; set; }

	public override Result Execute()
	{
		lock(Peering.Lock)
		{
			throw new NotImplementedException();
			//if(Peering.ValidateIncoming(Transaction, false, out var round))
			//{
			//	var b = round.AffectedUsers.Values.First(i => u == null ? i.Name == Transaction.User : i.Id == u.Id);
			//	
			//	var r = new ExaminePpr {};
			//
			//	if(u != null)
			//	{
			//		r.SpacetimeConsumed	= u.Spacetime - b.Spacetime;
			//		r.EnergyConsumed	= u.BandwidthExpiration > Mcv.LastConfirmedRound.ConsensusTime.Days ? 0 : Transaction.EnergyConsumed;
			//	}
			//	else
			//	{
			//		r.SpacetimeConsumed	= -b.Spacetime;
			//	}
			//
			//	return r;
			//}
			//else
			//	throw new EntityException(EntityError.ExcutionFailed);
		}
	}

	public override void Read(Reader reader)
	{
		Operations = reader.ReadArrayVirtual<Operation>();
		User = reader.ReadASCII();
	}

	public override void Write(Writer writer)
	{
		writer.WriteVirtual(Operations);
		writer.WriteASCII(User);
	}
}

public class ExaminePpr : Result
{
	public long			SpacetimeConsumed { get; set; }
	public long			EnergyConsumed { get; set; }

	public override void Read(Reader reader)
	{
		SpacetimeConsumed = reader.Read7BitEncodedInt64();
		EnergyConsumed = reader.Read7BitEncodedInt64();
	}

	public override void Write(Writer writer)
	{
		writer.Write7BitEncodedInt64(SpacetimeConsumed);
		writer.Write7BitEncodedInt64(EnergyConsumed);
	}
}
