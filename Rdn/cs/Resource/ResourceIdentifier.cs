namespace Uccs.Rdn;

public class ResourceIdentifier
{
	public Ura		Addres { get; set; }
	public AutoId	Id { get; set; }

	public static implicit operator Ura(ResourceIdentifier d) => d.Addres;
	public static implicit operator AutoId(ResourceIdentifier d) => d.Id;

	public ResourceIdentifier()
	{
	}

	public ResourceIdentifier(Ura addres)
	{
		Addres = addres;
	}

	public ResourceIdentifier(AutoId id)
	{
		Id = id;
	}
}
