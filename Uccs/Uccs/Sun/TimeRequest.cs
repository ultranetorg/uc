﻿namespace Uccs.Net
{
	public class TimeRequest : RdcRequest
	{
		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
			{
				if(!sun.Settings.Roles.HasFlag(Role.Base))					throw new RdcNodeException(RdcNodeError.NotBase);
				if(sun.Synchronization != Synchronization.Synchronized)	throw new RdcNodeException(RdcNodeError.NotSynchronized);
				
				return new TimeResponse {Time = sun.Mcv.LastConfirmedRound.ConfirmedTime};
			}
		}
	}

	public class TimeResponse : RdcResponse
	{
		public ChainTime Time { get; set; }
	}

}