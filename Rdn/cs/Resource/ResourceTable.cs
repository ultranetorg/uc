namespace Uccs.Rdn;

public class ResourceTable : Table<AutoId, Resource>
{
	public IEnumerable<RdnRound>	Tail => Mcv.Tail.Cast<RdnRound>();
	public new RdnMcv				Mcv => base.Mcv as RdnMcv;

	public ResourceTable(RdnMcv rds) : base(rds)
	{
	}
	
	public override Resource Create()
	{
		return new Resource(Mcv);
	}

	public override Resource Find(AutoId id, int ridmax)
	{
		var e = base.Find(id, ridmax);

		if(e == null)
			return null;

		e.Address.Domain = Mcv.Domains.Find(e.Domain, ridmax).Address;

		return e;
	}
	
	public Resource Find(Ura address, int ridmax)
	{
        var d = Mcv.Domains.Find(address.Domain, ridmax);

        if(d == null)
            return null;

  		foreach(var r in Tail.Where(i => i.Id <= ridmax))
 		{	
 			var x = r.AffectedResources.Values.FirstOrDefault(i => i.Address == address);
 					
 			if(x != null)
  				return x.Deleted ? null : x;
 		}

  		var e = FindBucket(d.Id.B)?.Entries.Find(i => i.Address == address);

		if(e == null)
			return null;

		e.Address.Domain = d.Address;

		return e;
	}
 }
