using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class SubresourcesRequest : RdcRequest
	{
		public ResourceAddress		Resource { get; set; }

		public override RdcResponse Execute(Core core)
		{
 			lock(core.Lock)
			{	
				if(core.Synchronization != Synchronization.Synchronized) throw new RdcNodeException(RdcNodeError.NotSynchronized);
 			
				
				return new SubresourcesResponse {Resources = core.Chainbase.Authors.EnumerateSubresources(Resource, core.Chainbase.LastConfirmedRound.Id).Select(i => i.Address.Resource).ToArray()};
			}
		}
	}
		
	public class SubresourcesResponse : RdcResponse
	{
		public IEnumerable<string> Resources { get; set; }
	}
}
