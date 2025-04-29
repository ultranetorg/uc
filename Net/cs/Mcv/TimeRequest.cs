namespace Uccs.Net;

public class TimeRequest : McvPpc<TimeResponse>
{
	public override PeerResponse Execute()
	{
		lock(Peering.Lock)
		{
			RequireGraph();
			
			return new TimeResponse {Time = Mcv.LastConfirmedRound.ConsensusTime};
		}
	}
}

public class TimeResponse : PeerResponse
{
	public Time Time { get; set; }
}
