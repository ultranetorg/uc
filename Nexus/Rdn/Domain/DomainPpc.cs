namespace Uccs.Rdn;

public class DomainPpc : RdnPpc<DomainPpr>
{
	public DomainIdentifier	Identifier { get; set; }

	public DomainPpc()
	{
	}

	public DomainPpc(DomainIdentifier identifier)
	{
		Identifier = identifier;
	}

	public DomainPpc(string addres)
	{
		Identifier = new(addres);
	}

	public DomainPpc(AutoId id)
	{
		Identifier = new(id);
	}

	public override Result Execute()
	{
		if(Identifier.Addres != null && !Domain.Valid(Identifier.Addres))	
			throw new RequestException(RequestError.IncorrectRequest);

 		lock(Mcv.Lock)
		{	
			RequireGraph();

			Domain e;

			if(Identifier.Addres != null)
				e = Mcv.Domains.Latest(Identifier.Addres);
			else if(Identifier.Id != null)
				e = Mcv.Domains.Latest(Identifier.Id);
			else
				throw new RequestException(RequestError.IncorrectRequest);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new DomainPpr {Domain = e};
		}
	}
}

public class DomainPpr : Result
{
	public Domain	Domain {get; set;}
}
