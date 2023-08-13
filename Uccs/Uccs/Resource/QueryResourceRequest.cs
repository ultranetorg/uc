using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class QueryResourceRequest : RdcRequest
	{
		public string		Query { get; set; }

		public override RdcResponse Execute(Sun sun)
		{
 			lock(sun.Lock)
			{	
				if(sun.Synchronization != Synchronization.Synchronized)
					throw new RdcNodeException(RdcNodeError.NotSynchronized);
 				
				return new QueryResourceResponse {Resources = sun.Mcv.QueryRelease(Query).Select(i => i.Address).ToArray()};
			}
		}
	}
		
	public class QueryResourceResponse : RdcResponse
	{
		public IEnumerable<ResourceAddress> Resources { get; set; }
	}
}
