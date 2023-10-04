using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Uccs.Net;

namespace Uccs.Net
{
	public class MembersRequest : RdcRequest
	{
		public override RdcResponse Execute(Sun sun)
		{
			lock(sun.Lock)
			{
				if(sun.Synchronization != Synchronization.Synchronized)
					throw new RdcNodeException(RdcNodeError.NotSynchronized);
			
				return new MembersResponse {Members = sun.Mcv.LastConfirmedRound.Members.Select(i => new MembersResponse.Member{Account = i.Account, 
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
			public IEnumerable<IPAddress>	BaseRdcIPs { get; set; }
			public IEnumerable<IPAddress>	SeedHubRdcIPs { get; set; }
			public bool         			Proxyable { get; set; }
		}

		public IEnumerable<Member> Members { get; set; }
	}
}
