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
				RequireSynchronizedBase(sun);
 				
				return new QueryResourceResponse {Resources = sun.Mcv.QueryResource(Query).Select(i => i.Address).ToArray()};
			}
		}
	}
		
	public class QueryResourceResponse : RdcResponse
	{
		public ResourceAddress[] Resources { get; set; }
	}
}
