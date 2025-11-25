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

	public override Return Execute()
	{
 		lock(Mcv.Lock)
		{	
			Resource r;

			if(Identifier.Addres != null)
				r = Mcv.Resources.Find(Identifier.Addres, Mcv.LastConfirmedRound.Id);
			else if(Identifier.Id != null)
				r = Mcv.Resources.Find(Identifier.Id, Mcv.LastConfirmedRound.Id);
			else
				throw new RequestException(RequestError.IncorrectRequest);
			
			if(r == null)
				throw new EntityException(EntityError.NotFound);
			
			return new ResourcePpr {Resource = r};
		}
	}
}
	
public class ResourcePpr : Return
{
	public Resource Resource { get; set; }
}
	
