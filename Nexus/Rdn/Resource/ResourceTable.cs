namespace Uccs.Rdn;

public class ResourceTable : Table<AutoId, Resource>
{
	public override string			Name => RdnTable.Resource.ToString();
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

		return e;
	}
	
	public Resource Find(Ura address, int ridmax)
	{
        var d = Mcv.Domains.Find(address.Domain, ridmax);

        if(d == null)
            return null;

  		foreach(var r in Tail.Where(i => i.Id <= ridmax))
 		{	
 			var x = r.Resources.Affected.Values.FirstOrDefault(i => i.Domain == d.Id && i.Name == address.Resource);
 					
 			if(x != null)
  				return x.Deleted ? null : x;
 		}

  		var e = FindBucket(d.Id.B)?.Entries.FirstOrDefault(i => i.Domain == d.Id && i.Name == address.Resource);

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
		var d = (Execution as RdnExecution).Domains.Find(address.Domain);

        if(d == null)
            return null;

		if(Affected.Values.FirstOrDefault(i => i.Domain == d.Id && i.Name == address.Resource) is Resource a && !a.Deleted)
			return a;

		return Table.Find(address, Execution.Round.Id);
	}

  	public Resource Affect(Domain domain, string name)
  	{
		var r =	Affected.Values.FirstOrDefault(i => i.Domain == domain.Id && i.Name == name);
		
		if(r != null && !r.Deleted)
			return r;

		r = Table.Find(new Ura(domain.Address, name), Execution.Round.Id);

  		if(r == null)
  		{
  			r = new Resource();
			r.Id = LastCreatedId =  new AutoId(domain.Id.B, Execution.GetNextEid(Table, domain.Id.B));
			r.Domain = domain.Id;
  			r.Name = name;
  		} 
  		else
			r = r.Clone() as Resource;
    
  		return Affected[r.Id] = r;
  	}

}
