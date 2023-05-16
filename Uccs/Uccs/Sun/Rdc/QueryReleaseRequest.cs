using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class QueryReleaseRequest : RdcRequest
	{
		public IEnumerable<ReleaseQuery>	Queries { get; set; }
		public bool							Confirmed { get; set; }

		public override RdcResponse Execute(Core core)
		{
 			lock(core.Lock)
			{	
				if(core.Synchronization != Synchronization.Synchronized)
					throw new RdcNodeException(RdcNodeError.NotSynchronized);
 				
				return new QueryReleaseResponse {Releases = Queries.Select(i => core.Database.QueryRelease(i)).ToArray()};
			}
		}
	}
		
	public class QueryReleaseResponse : RdcResponse
	{
		public IEnumerable<Release> Releases { get; set; }
	}
}
