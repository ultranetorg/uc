namespace Uccs.Rdn;

public class DomainByIdPpc : RdnPpc<DomainByIdPpr>
{
	public AutoId	Id { get; set; }

	public DomainByIdPpc()
	{
	}

	public DomainByIdPpc(AutoId id)
	{
		Id = id;
	}

	public override Result Execute()
	{
		var	r = Mcv.Domains.Latest(Id)
				??
				throw new EntityException(EntityError.NotFound);
			
		return new DomainByIdPpr {Domain = r};
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
	
public class DomainByIdPpr : Result
{
	public Domain Domain { get; set; }

	public override void Read(Reader reader)
	{
		Domain = reader.Read<Domain>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Domain);
	}

}

public class DomainByAddressPpc : RdnPpc<DomainByAddressPpr>
{
	public string		Address { get; set; }

	public DomainByAddressPpc()
	{
	}

	public DomainByAddressPpc(string address)
	{
		Address = address;
	}

	public override Result Execute()
	{
		var	r = Mcv.Domains.Latest(Address)
				??
				throw new EntityException(EntityError.NotFound);
			
		return new DomainByAddressPpr {Domain = r};
	}

	public override void Write(Writer writer)
	{
		writer.WriteASCII(Address);
	}

	public override void Read(Reader reader)
	{
		Address = reader.ReadASCII();
	}
}
	
public class DomainByAddressPpr : Result
{
	public Domain Domain { get; set; }

	public override void Read(Reader reader)
	{
		Domain = reader.Read<Domain>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Domain);
	}
}
	
