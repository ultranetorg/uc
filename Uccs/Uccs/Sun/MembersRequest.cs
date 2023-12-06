using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Uccs.Net
{
	public class MembersRequest : RdcRequest
	{
		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
			{
				RequireBase(sun);
			
				return new MembersResponse {Members = sun.Mcv.VotersOf(sun.Mcv.GetRound(sun.Mcv.LastConfirmedRound.Id + 1 + Mcv.P))
															 //.Where(i => i.CastingSince <= sun.Mcv.LastConfirmedRound.Id + Mcv.P)
															 .Select(i => new MembersResponse.Member {	Account = i.Account, 
																										CastingSince = i.CastingSince,
																										BaseRdcIPs = i.BaseRdcIPs, 
																										SeedHubRdcIPs = i.SeedHubRdcIPs, 
																										Proxyable = i.Proxy != null}).ToArray()};
			}
		}
	}

	public class MembersResponse : RdcResponse
	{
		public class Member
		{
			public AccountAddress			Account { get; set; }
			public int						CastingSince { get; set; }
			public IPAddress[]				BaseRdcIPs { get; set; }
			public IPAddress[]				SeedHubRdcIPs { get; set; }
			public bool         			Proxyable { get; set; }

			public override string ToString()
			{
				return $"{Account}";
			}
		}

		public Member[] Members { get; set; }
	}
}
