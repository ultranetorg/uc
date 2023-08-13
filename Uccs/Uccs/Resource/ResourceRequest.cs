using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class ResourceRequest : RdcRequest
	{
		public ResourceAddress		Resource { get; set; }

		public override RdcResponse Execute(Sun sun)
		{
 			lock(sun.Lock)
			{	
				if(sun.Synchronization != Synchronization.Synchronized) throw new RdcNodeException(RdcNodeError.NotSynchronized);
 			
				var r = sun.Mcv.Authors.FindResource(Resource, sun.Mcv.LastConfirmedRound.Id);
			
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
