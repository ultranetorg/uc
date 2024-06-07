namespace Uccs.Net
{
	public class TimeRequest : PeerCall<TimeResponse>
	{
		public override PeerResponse Execute()
		{
			lock(Mcv.Lock)
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

}
