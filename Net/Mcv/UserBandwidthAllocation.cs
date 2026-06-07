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

	public override void Read(Reader reader)
	{
		Bandwidth	= reader.ReadUInt16();
		Months		= reader.ReadByte();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Bandwidth);
		writer.Write(Months);
	}

	public override void Execute(Execution execution)
	{
		var now = execution.Time.Hours;
		var r = User.BandwidthExpiration - now;
		
		execution.AffectBandwidths();

		if(r > 0) /// reclaim the remaining
		{
			///User.Energy += User.Bandwidth * r;

			for(int i = 0; i < r; i++)
				execution.Bandwidths[now + i] -= User.Bandwidth;
		}
		
		var h = Months * 30 * 24;

		if(execution.Bandwidths.Length < now + h)
			execution.Bandwidths = [..execution.Bandwidths, ..new long[now + h - execution.Bandwidths.Length]];

		for(int i = 0; i < h; i++)
		{
			if(execution.Bandwidths[now + i] + Bandwidth <= execution.Net.EnergyHourlyEmission)
			{
				execution.Bandwidths[now + i] += Bandwidth;
			}
			else
			{
				Error = LimitExceeded;
				return;
			}
		}

		User.Energy					-= Bandwidth * h;
		User.Bandwidth				= Bandwidth;
		User.BandwidthExpiration	= execution.Time.Hours + h;

		execution.EnergySpenders.Add(User);
	}
}
