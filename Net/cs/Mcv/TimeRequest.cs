namespace Uccs.Net;

public class TimeRequest : McvPpc<TimeResponse>
{
	public override PeerResponse Execute()
	{
		lock(Peering.Lock)
		{
			RequireBase();
			
			return new TimeResponse {Time = Mcv.LastConfirmedRound.ConsensusTime};
		}
	}
}

public class TimeResponse : PeerResponse
{
	public Time Time { get; set; }
}
