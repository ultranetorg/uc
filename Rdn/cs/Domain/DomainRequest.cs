namespace Uccs.Rdn
{
	public class DomainRequest : RdnCall<DomainResponse>
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

		public override PeerResponse Execute()
		{
			if(Identifier.Addres != null && !Domain.Valid(Identifier.Addres))	
				throw new RequestException(RequestError.IncorrectRequest);

 			lock(Mcv.Lock)
			{	
				RequireBase();

				Domain e;

				if(Identifier.Addres != null)
					e = Mcv.Domains.Find(Identifier.Addres, Mcv.LastConfirmedRound.Id);
				else if(Identifier.Id != null)
					e = Mcv.Domains.Find(Identifier.Id, Mcv.LastConfirmedRound.Id);
				else
					throw new RequestException(RequestError.IncorrectRequest);
				
				if(e == null)
					throw new EntityException(EntityError.NotFound);
				
				return new DomainResponse {Domain = e};
			}
		}
	}
	
	public class DomainResponse : PeerResponse
	{
		public Domain	Domain {get; set;}
	}
}
