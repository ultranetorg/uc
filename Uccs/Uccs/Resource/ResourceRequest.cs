namespace Uccs.Net
{
	public class ResourceRequest : RdcRequest
	{
		public ResourceAddress		Resource { get; set; }

		public override RdcResponse Execute(Sun sun)
		{
 			lock(sun.Lock)
			{	
				RequireBase(sun);
 			
				var r = sun.Mcv.Authors.FindResource(Resource, sun.Mcv.LastConfirmedRound.Id);
			
				if(r == null)
					throw new RdcEntityException(RdcEntityError.NotFound);
				
				return new ResourceResponse {Resource = r};
			}
		}
	}
		
	public class ResourceResponse : RdcResponse
	{
		public Resource Resource { get; set; }
	}
}
