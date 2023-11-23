using System.Collections.Generic;

namespace Uccs.Net
{
	public class FundsRequest : RdcRequest
	{
		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
			{
				RequireBase(sun);
			
				return new FundsResponse {Funds = sun.Mcv.LastConfirmedRound.Funds.ToArray()};
			}
		}
	}

	public class FundsResponse : RdcResponse
	{
		public AccountAddress[] Funds { get; set; }
	}
}
