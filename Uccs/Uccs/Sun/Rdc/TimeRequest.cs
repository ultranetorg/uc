namespace Uccs.Net
{
	public class TimeRequest : RdcRequest
	{
		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
			{
				if(!core.Settings.Roles.HasFlag(Role.Base))					throw new RdcNodeException(RdcNodeError.NotBase);
				if(core.Synchronization != Synchronization.Synchronized)	throw new RdcNodeException(RdcNodeError.NotSynchronized);
				
				return new TimeResponse {Time = core.Database.LastConfirmedRound.Time};
			}
		}
	}

	public class TimeResponse : RdcResponse
	{
		public ChainTime Time { get; set; }
	}

}
