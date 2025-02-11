namespace Uccs.Rdn;

public class ResourceIdentifier
{
	public Ura		Addres { get; set; }
	public EntityId	Id { get; set; }

	public static implicit operator Ura(ResourceIdentifier d) => d.Addres;
	public static implicit operator EntityId(ResourceIdentifier d) => d.Id;

	public ResourceIdentifier()
	{
	}

	public ResourceIdentifier(Ura addres)
	{
		Addres = addres;
	}

	public ResourceIdentifier(EntityId id)
	{
		Id = id;
	}
}
