namespace Uccs.Rdn;

public class ResourceTable : Table<ResourceEntry>
{
	public IEnumerable<RdnRound>	Tail => Mcv.Tail.Cast<RdnRound>();
	public new RdnMcv				Mcv => base.Mcv as RdnMcv;

	public ResourceTable(RdnMcv rds) : base(rds)
	{
	}
	
	public override ResourceEntry Create()
	{
		return new ResourceEntry(Mcv);
	}

	public ResourceEntry Find(ResourceId id, int ridmax)
	{
		//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
		//	throw new IntegrityException("maxrid works inside pool only");

  			foreach(var i in Tail.Where(i => i.Id <= ridmax))
			if(i.AffectedResources.TryGetValue(id, out var r))
    				return r;

		var e = FindBucket(id.B)?.Entries.Find(i => i.Id.E == id.E && i.Id.R == id.R);

		if(e == null)
			return null;

		e.Address.Domain = Mcv.Domains.Find(id, ridmax).Address;

		return e;
	}
	
	public ResourceEntry Find(Ura address, int ridmax)
	{
 		//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
 		//	throw new IntegrityException("maxrid works inside pool only");

  		foreach(var r in Tail.Where(i => i.Id <= ridmax))
 		{	
 			var x = r.AffectedResources.Values.FirstOrDefault(i => i.Address == address);
 					
 			if(x != null)
  				return x;
 		}
 
        var d = Mcv.Domains.Find(address.Domain, ridmax);

        if(d == null)
            return null;

  		var e = FindBucket(d.Id.B)?.Entries.Find(i => i.Id.E == d.Id.E && i.Address.Resource == address.Resource);

		if(e == null)
			return null;

		e.Address.Domain = d.Address;

		return e;
	}
 }
