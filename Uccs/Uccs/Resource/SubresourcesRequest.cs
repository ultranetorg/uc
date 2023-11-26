using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class SubresourcesRequest : RdcRequest
	{
		public ResourceAddress		Resource { get; set; }

		protected override RdcResponse Execute(Sun sun)
		{
 			lock(sun.Lock)
			{	
				RequireBase(sun);
 							
				return new SubresourcesResponse {Resources = sun.Mcv.Authors.EnumerateSubresources(Resource, sun.Mcv.LastConfirmedRound.Id).Select(i => i.Address.Resource).ToArray()};
			}
		}
	}
		
	public class SubresourcesResponse : RdcResponse
	{
		public string[] Resources { get; set; }
	}
}
