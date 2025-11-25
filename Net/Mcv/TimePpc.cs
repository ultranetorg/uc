namespace Uccs.Net;

public class TimePpc : McvPpc<TimePpr>
{
	public override Return Execute()
	{
		lock(Peering.Lock)
		{
			RequireGraph();
			
			return new TimePpr {Time = Mcv.LastConfirmedRound.ConsensusTime};
		}
	}
}

public class TimePpr : Return
{
	public Time Time { get; set; }
}
