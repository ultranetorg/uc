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
				if(sun.Synchronization != Synchronization.Synchronized) throw new RdcNodeException(RdcNodeError.NotSynchronized);
			
				return new MembersResponse {Members = sun.Mcv.LastConfirmedRound.Members.Select(i => new MembersResponse.Member{ Account = i.Account, 
																																		HubIPs = i.HubIPs, 
																																		Proxyable = i.Proxy != null})};
			}
		}
	}

	public class MembersResponse : RdcResponse
	{
		public class Member
		{
			public AccountAddress			Account { get; set; }
			public IEnumerable<IPAddress>	HubIPs { get; set; }
			public bool         			Proxyable { get; set; }
		}

		public IEnumerable<Member> Members { get; set; }
	}
}
