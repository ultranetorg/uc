using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class SubresourcesRequest : RdcRequest
	{
		public ResourceAddress		Resource { get; set; }

		public override RdcResponse Execute(Sun sun)
		{
 			lock(sun.Lock)
			{	
				RequireSynchronizedBase(sun);
 							
				return new SubresourcesResponse {Resources = sun.Mcv.Authors.EnumerateSubresources(Resource, sun.Mcv.LastConfirmedRound.Id).Select(i => i.Address.Resource).ToArray()};
			}
		}
	}
		
	public class SubresourcesResponse : RdcResponse
	{
		public IEnumerable<string> Resources { get; set; }
	}
}
