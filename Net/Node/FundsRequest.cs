using System.Collections.Generic;

namespace Uccs.Net
{
	public class FundsRequest : RdcCall<FundsResponse>
	{
		public override RdcResponse Execute()
		{
			lock(Mcv.Lock)
			{
				RequireBase();
			
				return new FundsResponse {Funds = Mcv.LastConfirmedRound.Funds.ToArray()};
			}
		}
	}

	public class FundsResponse : RdcResponse
	{
		public AccountAddress[] Funds { get; set; }
	}
}
