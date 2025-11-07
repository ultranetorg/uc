namespace Uccs.Rdn;

public class DomainIdentifier
{
	public string			Addres { get; set; }
	public AutoId			Id { get; set; }

	public static implicit	operator string(DomainIdentifier d) => d.Addres;
	public static implicit	operator AutoId(DomainIdentifier d) => d.Id;

	public DomainIdentifier()
	{
	}

	public DomainIdentifier(string address)
	{
		Addres = address;
	}

	public DomainIdentifier(AutoId id)
	{
		Id = id;
	}
}
