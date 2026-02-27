namespace Uccs.Net;

public class TimePpc : McvPpc<TimePpr>
{
	public override Result Execute()
	{
		RequireGraph();
		
		lock(Mcv.Lock)
		{
			return new TimePpr {Time = Mcv.LastConfirmedRound.ConsensusTime};
		}
	}
}

public class TimePpr : Result
{
	public Time Time { get; set; }
}
