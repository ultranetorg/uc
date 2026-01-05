namespace Uccs.Net;

public class BandwidthAllocation : Operation
{
	public long				Bandwidth { get; set; }
	public short			Days  { get; set; }

	public override string	Explanation => $"Allocation of {Bandwidth} EC for {Days} days";
	public override bool	IsValid(McvNet net) => Bandwidth >= 0 && Days > 0 && Days <= net.BandwidthDaysMaximum;
	
	public BandwidthAllocation()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Bandwidth	= reader.Read7BitEncodedInt64();
		Days		= reader.ReadInt16();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write7BitEncodedInt64(Bandwidth);
		writer.Write(Days);
	}

	public override void Execute(Execution execution)
	{
		var r = User.BandwidthExpiration - execution.Time.Days;

		if(r > 0) /// reclaim the remaining
		{
			User.Energy += User.Bandwidth * r;

			for(int i = 0; i < r; i++)
				execution.Bandwidths[i] -= User.Bandwidth;
		}

		for(int i = 0; i < Days; i++)
		{
			if(execution.Bandwidths[i] + Bandwidth <= execution.Net.BandwidthAllocationPerDayMaximum)
			{
				execution.Bandwidths[i] += Bandwidth;
			}
			else
			{
				Error = LimitReached;
				return;
			}
		}

		User.Energy				-= Bandwidth * Days;
		User.Bandwidth			= Bandwidth;
		User.BandwidthExpiration	= (short)(execution.Time.Days + Days);
	}
}
