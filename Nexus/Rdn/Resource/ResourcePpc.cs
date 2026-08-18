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
			
		return new ResourceByIdPpr {Resource = r, Address = new Ura(Mcv.Domains.Latest(r.Domain).Name, r.Name)};
	}

	public override void Write(Writer writer)
	{
		writer.Write(Id);
	}

	public override void Read(Reader reader)
	{
		Id = reader.Read<AutoId>();
	}
}
	
public class ResourceByIdPpr : Result
{
	public Resource Resource { get; set; }
	public Ura		Address { get; set; }

	public override void Read(Reader reader)
	{
		Address = reader.Read<Ura>();
		Resource = reader.Read<Resource>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Address);
		writer.Write(Resource);
	}

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

	public override void Write(Writer writer)
	{
		writer.Write(Address);
	}

	public override void Read(Reader reader)
	{
		Address = reader.Read<Ura>();
	}
}
	
public class ResourceByAddressPpr : Result
{
	public Resource Resource { get; set; }

	public override void Read(Reader reader)
	{
		Resource = reader.Read<Resource>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Resource);
	}
}
	
