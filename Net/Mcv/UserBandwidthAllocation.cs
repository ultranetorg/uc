namespace Uccs.Net;

public class UserBandwidthAllocation : Operation
{
	public ushort			Bandwidth { get; set; }
	public byte				Months  { get; set; }

	public override string	Explanation => $"Allocation of {Bandwidth} bandwidth for {Months} months";
	public override bool	IsValid(McvNet net) => Bandwidth >= 0 && Months > 0 && Months <= McvNet.BandwidthRentMonthsMaximum;
	
	public UserBandwidthAllocation()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Bandwidth	= reader.ReadUInt16();
		Months		= reader.ReadByte();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Bandwidth);
		writer.Write(Months);
	}

	public override void Execute(Execution execution)
	{
		var r = User.BandwidthExpiration - execution.Time.Hours;
		
		execution.AffectBandwidths();

		if(r > 0) /// reclaim the remaining
		{
			///User.Energy += User.Bandwidth * r;

			for(int i = 0; i < r; i++)
				execution.Bandwidths[i] -= User.Bandwidth;
		}
		
		var h = Months * 30 * 24;

		for(int i = 0; i < h; i++)
		{
			if(execution.Bandwidths[i] + Bandwidth <= execution.Net.EnergyHourlyEmission)
			{
				execution.Bandwidths[i] += Bandwidth;
			}
			else
			{
				Error = LimitReached;
				return;
			}
		}

		User.Energy					-= Bandwidth * h;
		User.Bandwidth				= Bandwidth;
		User.BandwidthExpiration	= execution.Time.Hours + h;

		execution.EnergySpenders.Add(User);
	}
}
