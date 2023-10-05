using System.Collections.Generic;

namespace Uccs.Net
{
	public class FundsRequest : RdcRequest
	{
		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
			{
				if(sun.Synchronization != Synchronization.Synchronized)
					throw new RdcNodeException(RdcNodeError.NotSynchronized);
			
				return new FundsResponse {Funds = sun.Mcv.LastConfirmedRound.Funds.ToArray()};
			}
		}
	}

	public class FundsResponse : RdcResponse
	{
		public IEnumerable<AccountAddress> Funds { get; set; }
	}
}
