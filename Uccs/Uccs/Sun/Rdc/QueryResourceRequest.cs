using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class QueryResourceRequest : RdcRequest
	{
		public string		Query { get; set; }

		public override RdcResponse Execute(Core core)
		{
 			lock(core.Lock)
			{	
				if(core.Synchronization != Synchronization.Synchronized)
					throw new RdcNodeException(RdcNodeError.NotSynchronized);
 				
				return new QueryResourceResponse {Resources = core.Chainbase.QueryRelease(Query).ToArray()};
			}
		}
	}
		
	public class QueryResourceResponse : RdcResponse
	{
		public IEnumerable<Resource> Resources { get; set; }
	}
}
