namespace Uccs.Net
{
	public class TimeRequest : RdcCall<TimeResponse>
	{
		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
			{
				RequireBase(sun);
				
				return new TimeResponse {Time = sun.Mcv.LastConfirmedRound.ConsensusTime};
			}
		}
	}

	public class TimeResponse : RdcResponse
	{
		public Time Time { get; set; }
	}

}
