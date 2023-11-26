namespace Uccs.Net
{
	public class TimeRequest : RdcRequest
	{
		protected override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
			{
				RequireBase(sun);
				
				return new TimeResponse {Time = sun.Mcv.LastConfirmedRound.ConfirmedTime};
			}
		}
	}

	public class TimeResponse : RdcResponse
	{
		public Time Time { get; set; }
	}

}
