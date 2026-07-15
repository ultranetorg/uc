namespace Uccs.Rdn;

public class ResourceByIdPpc : RdnPpc<ResourceByIdPpr>
{
	public AutoId	Id { get; set; }

	public ResourceByIdPpc()
	{
	}

	public ResourceByIdPpc(AutoId id)
	{
		Id = id;
	}

	public override Result Execute()
	{
		var	r = Mcv.Resources.Latest(Id)
				??
				throw new EntityException(EntityError.NotFound);
			
		return new ResourceByIdPpr {Resource = r, Address = new Ura(Mcv.Domains.Latest(r.Domain).Address, r.Name)};
	}
}
	
public class ResourceByIdPpr : Result
{
	public Resource Resource { get; set; }
	public Ura		Address { get; set; }
}
	

public class ResourceByAddressPpc : RdnPpc<ResourceByAddressPpr>
{
	public Ura		Address { get; set; }

	public ResourceByAddressPpc()
	{
	}

	public ResourceByAddressPpc(Ura address)
	{
		Address = address;
	}

	public override Result Execute()
	{
		var	r = Mcv.Resources.Latest(Address)
				??
				throw new EntityException(EntityError.NotFound);
			
		return new ResourceByAddressPpr {Resource = r};
	}
}
	
public class ResourceByAddressPpr : Result
{
	public Resource Resource { get; set; }
}
	
