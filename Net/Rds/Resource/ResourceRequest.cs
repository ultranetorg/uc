namespace Uccs.Net
{
	public class ResourceRequest : RdcCall<ResourceResponse>
	{
		public ResourceIdentifier	Identifier { get; set; }

		public ResourceRequest()
		{
		}

		public ResourceRequest(ResourceIdentifier identifier)
		{
			Identifier = identifier;
		}

		public ResourceRequest(Ura addres)
		{
			Identifier = new(addres);
		}

		public ResourceRequest(ResourceId id)
		{
			Identifier = new(id);
		}

		public override RdcResponse Execute(Sun sun)
		{
 			lock(sun.Lock)
			{	
				Resource r;

				var rds = RequireRdsBase(sun);

				if(Identifier.Addres != null)
					r = rds.Domains.FindResource(Identifier.Addres, sun.Mcv.LastConfirmedRound.Id);
				else if(Identifier.Id != null)
					r = rds.Domains.FindResource(Identifier.Id, sun.Mcv.LastConfirmedRound.Id);
				else
					throw new RequestException(RequestError.IncorrectRequest);
				
				if(r == null)
					throw new EntityException(EntityError.NotFound);
				
				return new ResourceResponse {Resource = r};
			}
		}
	}
		
	public class ResourceResponse : RdcResponse
	{
		public Resource Resource { get; set; }
	}
		
}
