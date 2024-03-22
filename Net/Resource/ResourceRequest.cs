namespace Uccs.Net
{
	public class ResourceByNameRequest : RdcRequest
	{
		public ResourceAddress	Name { get; set; }

		public override RdcResponse Execute(Sun sun)
		{
 			lock(sun.Lock)
			{	
				RequireBase(sun);
 			
				var r = sun.Mcv.Authors.FindResource(Name, sun.Mcv.LastConfirmedRound.Id);
			
				if(r == null)
					throw new EntityException(EntityError.NotFound);
				
				return new ResourceByNameResponse {Resource = r};
			}
		}
	}

	public class ResourceByIdRequest : RdcRequest
	{
		public ResourceId	ResourceId { get; set; }

		public override RdcResponse Execute(Sun sun)
		{
 			lock(sun.Lock)
			{	
				RequireBase(sun);
 			
				var r = sun.Mcv.Authors.FindResource(ResourceId, sun.Mcv.LastConfirmedRound.Id);
			
				if(r == null)
					throw new EntityException(EntityError.NotFound);
				
				return new ResourceByIdResponse {Resource = r};
			}
		}
	}
		
	public class ResourceByNameResponse : RdcResponse
	{
		public Resource Resource { get; set; }
	}
		
	public class ResourceByIdResponse : RdcResponse
	{
		public Resource Resource { get; set; }
	}
}
