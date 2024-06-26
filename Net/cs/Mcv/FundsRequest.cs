using System.Collections.Generic;

namespace Uccs.Net
{
	public class FundsRequest : McvCall<FundsResponse>
	{
		public override PeerResponse Execute()
		{
			lock(Mcv.Lock)
			{
				RequireBase();
			
				return new FundsResponse {Funds = Mcv.LastConfirmedRound.Funds.ToArray()};
			}
		}
	}

	public class FundsResponse : PeerResponse
	{
		public AccountAddress[] Funds { get; set; }
	}
}
