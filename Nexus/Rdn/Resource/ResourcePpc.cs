namespace Uccs.Rdn;

public class ResourcePpc : RdnPpc<ResourcePpr>
{
	public ResourceIdentifier	Identifier { get; set; }

	public ResourcePpc()
	{
	}

	public ResourcePpc(ResourceIdentifier identifier)
	{
		Identifier = identifier;
	}

	public ResourcePpc(Ura addres)
	{
		Identifier = new(addres);
	}

	public ResourcePpc(AutoId id)
	{
		Identifier = new(id);
	}

	public override Result Execute()
	{
 		lock(Mcv.Lock)
		{	
			Resource r;

			if(Identifier.Addres != null)
				r = Mcv.Resources.Find(Identifier.Addres, Mcv.LastConfirmedRound.Id);
			else if(Identifier.Id != null)
				r = Mcv.Resources.Latest(Identifier.Id);
			else
				throw new RequestException(RequestError.IncorrectRequest);
			
			if(r == null)
				throw new EntityException(EntityError.NotFound);
			
			return new ResourcePpr {Resource = r, Address = new Ura(Mcv.Domains.Latest(r.Domain).Address, r.Name)};
		}
	}
}
	
public class ResourcePpr : Result
{
	public Resource Resource { get; set; }
	public Ura		Address { get; set; }
}
	
