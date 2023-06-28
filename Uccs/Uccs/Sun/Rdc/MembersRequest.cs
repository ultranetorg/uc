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
		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
			{
				if(core.Synchronization != Synchronization.Synchronized) throw new RdcNodeException(RdcNodeError.NotSynchronized);
			
				return new MembersResponse {Members = core.Database.LastConfirmedRound.Generators.Select(i => new MembersResponse.Member {	Account = i.Account, 
																																			PublicIPs = i.IPs, 
																																			Proxyable = i.Proxy != null})};
			}
		}
	}

	public class MembersResponse : RdcResponse
	{
		public class Member
		{
			public AccountAddress			Account { get; set; }
			public IEnumerable<IPAddress>	PublicIPs { get; set; }
			public bool         			Proxyable { get; set; }
		}

		public IEnumerable<Member> Members {get; set;}
	}
}
