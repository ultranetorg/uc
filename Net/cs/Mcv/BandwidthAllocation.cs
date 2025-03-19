namespace Uccs.Net;

public class BandwidthAllocation : Operation
{
	public long				Bandwidth { get; set; }
	public short			Days  { get; set; }

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

	public override void Execute(Execution execution, Round round)
	{
		var r = Signer.BandwidthExpiration - round.ConsensusTime.Days;

		if(r > 0) /// reclaim the remaining
		{
			Signer.Energy += Signer.Bandwidth * r;

			for(int i = 0; i < r; i++)
				round.BandwidthAllocations[i] -= Signer.Bandwidth;
		}

		for(int i = 0; i < Days; i++)
		{
			if(round.BandwidthAllocations[i] + Bandwidth <= execution.Net.BandwidthAllocationPerDayMaximum)
			{
				round.BandwidthAllocations[i] += Bandwidth;
			}
			else
			{
				Error = LimitReached;
				return;
			}
		}

		Signer.Energy				-= Bandwidth * Days;
		Signer.Bandwidth			= Bandwidth;
		Signer.BandwidthExpiration	= (short)(round.ConsensusTime.Days + Days);
	}
}
