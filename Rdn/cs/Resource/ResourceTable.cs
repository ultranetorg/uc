using System.Diagnostics;

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

	public ResourceEntry Find(EntityId id, int ridmax)
	{
  		foreach(var i in Tail.Where(i => i.Id <= ridmax))
			if(i.AffectedResources.TryGetValue(id, out var r) && !r.Deleted)
   				return r;

		var e = FindBucket(id.B)?.Entries.Find(i => i.Id.E == id.E);

		if(e == null || e.Deleted)
			return null;

		e.Address.Domain = Mcv.Domains.Find(e.Domain, ridmax).Address;

		return e;
	}
	
	public ResourceEntry Find(Ura address, int ridmax)
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
