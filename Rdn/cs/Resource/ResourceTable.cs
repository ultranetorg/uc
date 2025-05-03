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
 			var x = r.Resources.Affected.Values.FirstOrDefault(i => i.Address == address);
 					
 			if(x != null)
  				return x.Deleted ? null : x;
 		}

  		var e = FindBucket(d.Id.B)?.Entries.FirstOrDefault(i => i.Address == address);

		if(e == null)
			return null;

		e.Address.Domain = d.Address;

		return e;
	}
 }

public class ResourceExecution : TableExecution<AutoId, Resource>
{
	new ResourceTable Table => base.Table as ResourceTable;
		
	public ResourceExecution(RdnExecution execution) : base(execution.Mcv.Resources, execution)
	{
	}

	public Resource Find(Ura address)
	{
		if(Affected.Values.FirstOrDefault(i => i.Address == address) is Resource a && !a.Deleted)
			return a;

		return Table.Find(address, Execution.Round.Id);
	}

  	public Resource Affect(Domain domain, string resource)
  	{
		var r =	Affected.Values.FirstOrDefault(i => i.Address.Domain == domain.Address && i.Address.Resource == resource);
		
		if(r != null && !r.Deleted)
			return r;

		r = Table.Find(new Ura(domain.Address, resource), Execution.Round.Id);

  		if(r == null)
  		{
  			r = new Resource{Id = new AutoId(domain.Id.B, Execution.GetNextEid(Table, domain.Id.B)),
							 Domain = domain.Id,
  							 Address = new Ura(domain.Address, resource)};
  		} 
  		else
			r = r.Clone() as Resource;
    
  		return Affected[r.Id] = r;
  	}

}
