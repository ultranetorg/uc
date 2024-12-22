namespace Uccs.Net;

public class BandwidthAllocation : Operation
{
	public long				Bandwidth;
	public short			Days;
	public override string	Description => $"Allocation of {Bandwidth} EC for {Days} days";
	public override bool	IsValid(Mcv mcv) => Bandwidth >= 0 && Days > 0 && Days <= mcv.Net.BandwidthAllocationDaysMaximum;
	
	public BandwidthAllocation()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Bandwidth	= reader.Read7BitEncodedInt64();
		Days		= reader.ReadInt16();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write7BitEncodedInt64(Bandwidth);
		writer.Write(Days);
	}

	public override void Execute(Mcv mcv, Round round)
	{
		if(Signer.BandwidthExpiration > round.ConsensusTime) /// reclaim
		{
			var d = (Signer.BandwidthExpiration - round.ConsensusTime).Days;
			 
			/// Signer.ECBalance += Signer.BandwidthNext * d / 2;  /// refund 50% for what is left

			for(int i=1; i<=d; i++)
				round.NextBandwidthAllocations[i] -= Signer.BandwidthNext;
		}

		for(int i=1; i<=Days; i++)
		{
			if(round.NextBandwidthAllocations[i] + Bandwidth < mcv.Net.BandwidthAllocationPerDayMaximum)
			{
				round.NextBandwidthAllocations[i] += Bandwidth;
			}
			else
			{
				Error = LimitReached;
				return;
			}
		}

		Signer.BandwidthNext		= Bandwidth;
		Signer.BandwidthExpiration	= round.ConsensusTime + Time.FromDays(Days);
		
		Transaction.ECSpent		+= Bandwidth * Days;
		//Transaction.ECReward	+= Bandwidth * Days;
	}
}
