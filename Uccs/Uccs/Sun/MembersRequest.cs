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
				RequireSynchronizedBase(sun);
			
				return new MembersResponse {Members = sun.Mcv.LastConfirmedRound.Members.Where(i => i.CastingSince <= sun.Mcv.LastConfirmedRound.Id + Mcv.P)
																						.Select(i => new MembersResponse.Member{Account = i.Account, 
																																JoinedAt = i.CastingSince,
																																BaseRdcIPs = i.BaseRdcIPs, 
																																SeedHubRdcIPs = i.SeedHubRdcIPs, 
																																Proxyable = i.Proxy != null})};
			}
		}
	}

	public class MembersResponse : RdcResponse
	{
		public class Member
		{
			public AccountAddress			Account { get; set; }
			public int						JoinedAt { get; set; }
			public IEnumerable<IPAddress>	BaseRdcIPs { get; set; }
			public IEnumerable<IPAddress>	SeedHubRdcIPs { get; set; }
			public bool         			Proxyable { get; set; }

			public override string ToString()
			{
				return $"{Account}";
			}
		}

		public IEnumerable<Member> Members { get; set; }
	}
}
