using System.Linq;

namespace Uccs.Net
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

 			lock(Rdn.Lock)
			{	
				RequireBase();

				Domain e;

				if(Identifier.Addres != null)
					e = Rdn.Domains.Find(Identifier.Addres, Rdn.LastConfirmedRound.Id);
				else if(Identifier.Id != null)
					e = Rdn.Domains.Find(Identifier.Id, Rdn.LastConfirmedRound.Id);
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
