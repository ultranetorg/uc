namespace Uccs.Rdn
{
	public class ResourceRequest : RdnPpc<ResourceResponse>
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
					r = Mcv.Domains.FindResource(Identifier.Addres, Mcv.LastConfirmedRound.Id);
				else if(Identifier.Id != null)
					r = Mcv.Domains.FindResource(Identifier.Id, Mcv.LastConfirmedRound.Id);
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
