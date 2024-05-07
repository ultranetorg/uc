namespace Uccs.Net
{
	public class DomainRequest : RdcCall<DomainResponse>
	{
		public DomainIdentifier	Identifier { get; set; }

		public DomainRequest()
		{
		}

		public DomainRequest(DomainIdentifier identifier)
		{
			Identifier = identifier;
		}

		public DomainRequest(string addres)
		{
			Identifier = new(addres);
		}

		public DomainRequest(EntityId id)
		{
			Identifier = new(id);
		}

		public override RdcResponse Execute(Sun sun)
		{
			if(Identifier.Addres != null && !Domain.Valid(Identifier.Addres))	
				throw new RequestException(RequestError.IncorrectRequest);

 			lock(sun.Lock)
			{	
				RequireBase(sun);

				Domain e;

				if(Identifier.Addres != null)
					e = sun.Mcv.Domains.Find(Identifier.Addres, sun.Mcv.LastConfirmedRound.Id);
				else if(Identifier.Id != null)
					e = sun.Mcv.Domains.Find(Identifier.Id, sun.Mcv.LastConfirmedRound.Id);
				else
					throw new RequestException(RequestError.IncorrectRequest);
				
				if(e == null)
					throw new EntityException(EntityError.NotFound);
				
				return new DomainResponse {Domain = e};
			}
		}
	}
	
	public class DomainResponse : RdcResponse
	{
		public Domain	Domain {get; set;}
	}
}
