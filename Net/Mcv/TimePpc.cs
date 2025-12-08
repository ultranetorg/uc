namespace Uccs.Net;

public class TimePpc : McvPpc<TimePpr>
{
	public override Result Execute()
	{
		lock(Peering.Lock)
		{
			RequireGraph();
			
			return new TimePpr {Time = Mcv.LastConfirmedRound.ConsensusTime};
		}
	}
}

public class TimePpr : Result
{
	public Time Time { get; set; }
}
