namespace Uccs.Net;

public class TimePpc : McvPpc<TimePpr>
{
	public override PeerResponse Execute()
	{
		lock(Peering.Lock)
		{
			RequireGraph();
			
			return new TimePpr {Time = Mcv.LastConfirmedRound.ConsensusTime};
		}
	}
}

public class TimePpr : PeerResponse
{
	public Time Time { get; set; }
}
