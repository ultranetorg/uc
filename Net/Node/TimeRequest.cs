namespace Uccs.Net
{
	public class TimeRequest : RdcCall<TimeResponse>
	{
		public override RdcResponse Execute()
		{
			lock(Mcv.Lock)
			{
				RequireBase();
				
				return new TimeResponse {Time = Mcv.LastConfirmedRound.ConsensusTime};
			}
		}
	}

	public class TimeResponse : RdcResponse
	{
		public Time Time { get; set; }
	}

}
