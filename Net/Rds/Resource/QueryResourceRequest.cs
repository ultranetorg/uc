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
				var rds = RequireRdsBase(sun);

				return new QueryResourceResponse {Resources = rds.QueryResource(Query).Select(i => i.Address).ToArray()};
			}
		}
	}
		
	public class QueryResourceResponse : RdcResponse
	{
		public Ura[] Resources { get; set; }
	}
}
