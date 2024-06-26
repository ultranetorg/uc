namespace Uccs.Rdn
{
	public class ResourceRequest : RdnCall<ResourceResponse>
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

		public override PeerResponse Execute()
		{
 			lock(Mcv.Lock)
			{	
				Resource r;

				if(Identifier.Addres != null)
					r = Rdn.Domains.FindResource(Identifier.Addres, Rdn.LastConfirmedRound.Id);
				else if(Identifier.Id != null)
					r = Rdn.Domains.FindResource(Identifier.Id, Rdn.LastConfirmedRound.Id);
				else
					throw new RequestException(RequestError.IncorrectRequest);
				
				if(r == null)
					throw new EntityException(EntityError.NotFound);
				
				return new ResourceResponse {Resource = r};
			}
		}
	}
		
	public class ResourceResponse : PeerResponse
	{
		public Resource Resource { get; set; }
	}
		
}
