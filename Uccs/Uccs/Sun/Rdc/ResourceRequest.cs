using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class ResourceRequest : RdcRequest
	{
		public ResourceAddress		Resource { get; set; }

		public override RdcResponse Execute(Core core)
		{
 			lock(core.Lock)
			{	
				if(core.Synchronization != Synchronization.Synchronized) throw new RdcNodeException(RdcNodeError.NotSynchronized);
 			
				var r = core.Chainbase.Resources.Find(Resource, core.Chainbase.LastConfirmedRound.Id);
			
				if(r == null)
					throw new RdcEntityException(RdcEntityError.ResourceNotFound);
				
				return new ResourceResponse {Resource = r};
			}
		}
	}
		
	public class ResourceResponse : RdcResponse
	{
		public Resource Resource { get; set; }
	}
}
