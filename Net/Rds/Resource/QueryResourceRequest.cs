using System.Linq;

namespace Uccs.Net
{
	public class QueryResourceRequest : RdsCall<QueryResourceResponse>
	{
		public string		Query { get; set; }

		public override RdcResponse Execute()
		{
 			lock(Rds.Lock)
			{	
				return new QueryResourceResponse {Resources = Rds.QueryResource(Query).Select(i => i.Address).ToArray()};
			}
		}
	}
		
	public class QueryResourceResponse : RdcResponse
	{
		public Ura[] Resources { get; set; }
	}
}
