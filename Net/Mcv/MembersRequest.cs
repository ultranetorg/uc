using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Uccs.Net
{
	public class MembersRequest : PeerCall<MembersResponse>
	{
		public override PeerResponse Execute()
		{
			lock(Mcv.Lock)
			{
				RequireBase();
			
				if(Mcv.NextVoteMembers.Count == 0)
					throw new EntityException(EntityError.NoMembers);

				return new MembersResponse {Members = Mcv.NextVoteMembers//.Where(i => i.CastingSince <= sun.Mcv.LastConfirmedRound.Id + Mcv.P)
																		 .Select(i => new MembersResponse.Member {	Account = i.Account, 
																													CastingSince = i.CastingSince,
																													BaseRdcIPs = i.BaseRdcIPs, 
																													SeedHubRdcIPs = i.SeedHubRdcIPs, 
																													Proxyable = i.Proxy != null}).ToArray()};
			}
		}
	}

	public class MembersResponse : PeerResponse
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
