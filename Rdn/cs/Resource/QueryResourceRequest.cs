namespace Uccs.Rdn
{
	public class QueryResourceRequest : RdnCall<QueryResourceResponse>
	{
		public string		Query { get; set; }

		public override PeerResponse Execute()
		{
 			lock(Rdn.Lock)
			{	
				return new QueryResourceResponse {Resources = Rdn.QueryResource(Query).Select(i => i.Address).ToArray()};
			}
		}
	}
		
	public class QueryResourceResponse : PeerResponse
	{
		public Ura[] Resources { get; set; }
	}
}
