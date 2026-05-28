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
 		lock(Mcv.Lock)
		{	
			var	r = Mcv.Resources.Latest(Id)
					??
					throw new EntityException(EntityError.NotFound);
			
			return new ResourceByIdPpr {Resource = r, Address = new Ura(Mcv.Domains.Latest(r.Domain).Address, r.Name)};
		}
	}
}
	
public class ResourceByIdPpr : Result
{
	public Resource Resource { get; set; }
	public Ura		Address { get; set; }
}
	

public class ResourceByAddressPpc : RdnPpc<ResourceByAddressPpr>
{
	public Ura		Addres { get; set; }

	public ResourceByAddressPpc()
	{
	}

	public ResourceByAddressPpc(Ura addres)
	{
		Addres = addres;
	}

	public override Result Execute()
	{
 		lock(Mcv.Lock)
		{	
			var	r = Mcv.Resources.Latest(Addres)
					??
					throw new EntityException(EntityError.NotFound);
			
			return new ResourceByAddressPpr {Resource = r, Address = new Ura(Mcv.Domains.Latest(r.Domain).Address, r.Name)};
		}
	}
}
	
public class ResourceByAddressPpr : Result
{
	public Resource Resource { get; set; }
	public Ura		Address { get; set; }
}
	
