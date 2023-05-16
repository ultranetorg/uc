using System;
using System.Collections.Generic;
using System.Linq;
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
			
				return new MembersResponse {Members = core.Database.LastConfirmedRound.Members.Select(i => i.Generator).ToArray()};
			}
		}
	}

	public class MembersResponse : RdcResponse
	{
		public IEnumerable<AccountAddress> Members {get; set;}
	}
}
