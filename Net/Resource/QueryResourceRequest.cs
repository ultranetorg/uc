using System.Linq;

namespace Uccs.Net
{
	public class QueryResourceRequest : RdcCall<QueryResourceResponse>
	{
		public string		Query { get; set; }

		public override RdcResponse Execute(Sun sun)
		{
 			lock(sun.Lock)
			{	
				RequireBase(sun);
 				
				return new QueryResourceResponse {Resources = sun.Mcv.QueryResource(Query).Select(i => i.Address).ToArray()};
			}
		}
	}
		
	public class QueryResourceResponse : RdcResponse
	{
		public ResourceAddress[] Resources { get; set; }
	}
}
