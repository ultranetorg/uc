
namespace Uccs.Net;

public class TimePpc : McvPpc<TimePpr>
{
	public override Result Execute()
	{
		RequireGraph();
		
		return new TimePpr {Time = Mcv.LastConfirmedRound.ConsensusTime};
	}

	public override void Read(Reader reader)
	{
	}

	public override void Write(Writer writer)
	{
	}
}

public class TimePpr : Result
{
	public Time Time { get; set; }

	public override void Read(Reader reader)
	{
		Time = reader.Read<Time>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Time);
	}
}
