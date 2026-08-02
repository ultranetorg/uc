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

	//public override Resource Find(AutoId id, int ridmax)
	//{
	//	var e = base.Find(id, ridmax);
	//
	//	if(e == null)
	//		return null;
	//
	//	return e;
	//}
	
	public Resource Find(Ura address)
	{
        var d = Mcv.Domains.Find(address.Domain);

        if(d == null)
            return null;

 		//var r = (Mcv.LastConfirmedRound as RdnRound).Resources.Affected.Values.FirstOrDefault(i => i.Domain == d.Id && i.Name == address.Resource);
		//
		//if(r != null)
		//	return r.Deleted ? null : r;

  		return FindBucket(d.Id.B)?.Entries.FirstOrDefault(i => i.Domain == d.Id && i.Name == address.Resource);
	}

	public virtual Resource Latest(Ura name)
	{
	     var d = Mcv.Domains.Latest(name.Domain);

        if(d == null)
            return null;

		var e = (Mcv.LastConfirmedRound as RdnRound).Resources.Affected.Values.FirstOrDefault(i => i.Domain == d.Id && i.Name == name.Resource);

		if(e != null)
			return e.Deleted ? null : e;

		return Find(name);
	}
 }

public class ResourceExecution : TableExecution<AutoId, Resource>
{
	new ResourceTable		Table => base.Table as ResourceTable;
	new RdnExecution		Execution=> base.Execution as RdnExecution;
		
	public ResourceExecution(RdnExecution execution) : base(execution.Mcv.Resources, execution)
	{
	}

	public Resource Find(Domain domain, Ura address)
	{
		var r = Affected.Values.FirstOrDefault(i => i.Domain == domain.Id && i.Name == address.Resource);
		
		if(r != null)
			return r.Deleted ? null : r;

		if(Parent != null)
			return (Parent as ResourceExecution).Find(domain, address);

 		r = Execution.Round.Resources.Affected.Values.FirstOrDefault(i => i.Domain == domain.Id && i.Name == address.Resource);
		
		if(r != null)
			return r.Deleted ? null : r;

		return Table.Find(address);
	}

  	public Resource Affect(Domain domain, Ura adddres)
  	{
		if(Affected.Values.FirstOrDefault(i => i.Domain == domain.Id && i.Name == adddres.Resource) is Resource r)
			return r.Deleted ? null : r;

		if(Parent != null)
			return (Parent as ResourceExecution).Find(domain, adddres);

		r = Execution.Round.Resources.Affected.Values.FirstOrDefault(i => i.Domain == domain.Id && i.Name == adddres.Resource);

		if(r == null)
			r = Table.Find(adddres);

  		if(r == null)
  		{
  			r = new Resource()
				{
					Id		= LastCreatedId = new AutoId(domain.Id.B, Execution.GetNextEid(Table, domain.Id.B)),
					Domain	= domain.Id,
  					Name	= adddres.Resource
				};
  		} 
  		else
			r = r.Clone() as Resource;
    
  		return Affected[r.Id] = r;
  	}
}
