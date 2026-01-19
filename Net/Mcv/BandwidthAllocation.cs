namespace Uccs.Net;

public class BandwidthAllocation : Operation
{
	public ushort			Bandwidth { get; set; }
	public ushort			Days  { get; set; }

	public override string	Explanation => $"Allocation of {Bandwidth} bandwidth for {Days} days";
	public override bool	IsValid(McvNet net) => Bandwidth >= 0 && Days > 0 && Days <= net.BandwidthDaysMaximum;
	
	public BandwidthAllocation()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Bandwidth	= reader.ReadUInt16();
		Days		= reader.ReadUInt16();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Bandwidth);
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
			if(execution.Bandwidths[i] + Bandwidth <= execution.Net.EnergyDayEmission)
			{
				execution.Bandwidths[i] += Bandwidth;
			}
			else
			{
				Error = LimitReached;
				return;
			}
		}

		User.Energy					-= Bandwidth * Days;
		User.Bandwidth				= Bandwidth;
		User.BandwidthExpiration	= (short)(execution.Time.Days + Days);
	}
}
