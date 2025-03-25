namespace Uccs.Rdn;

public class RdnExecution : Execution
{
	public new Rdn				Net => base.Net as Rdn;
	public new RdnMcv			Mcv => base.Mcv as RdnMcv;
	public new RdnRound			Round => base.Round as RdnRound;

	public Dictionary<EntityId, Domain>		AffectedDomains = [];
	public Dictionary<EntityId, Resource>		AffectedResources = [];

	public RdnExecution(RdnMcv mcv, RdnRound round, Transaction transaction) : base(mcv, round, transaction)
	{
	}

	public override ITableEntry Affect(byte table, BaseId id)
	{
		if(Mcv.Domains.Id == table)		return FindDomain(id as EntityId) != null ? AffectDomain(id as EntityId) : null;
		if(Mcv.Resources.Id == table)	return FindResource(id as EntityId) != null ? AffectResource(id as EntityId) : null;

		return base.Affect(table, id);
	}

	public Domain FindDomain(EntityId id)
	{
		if(AffectedDomains.TryGetValue(id, out var a))
			return a;

		return Mcv.Domains.Find(id, Round.Id);
	}

	public Domain FindDomain(string name)
	{
		if(AffectedDomains.Values.FirstOrDefault(i => i.Address == name) is Domain a)
			return a;

		return Mcv.Domains.Find(name, Round.Id);
	}

	public Domain AffectDomain(string address)
	{
		if(AffectedDomains.Values.FirstOrDefault(i => i.Address == address) is Domain d && !d.Deleted)
			return d;
		
		d = Mcv.Domains.Find(address, Round.Id);

		if(d != null)
			return AffectedDomains[d.Id] = d.Clone();
		else
		{
			var b = Mcv.Domains.KeyToBid(address);
			
			int e = GetNextEid(Mcv.Domains, b);

			d = new Domain(Mcv) {Id = new EntityId(b, e), Address = address};

			return AffectedDomains[d.Id] = d;
		}
	}

	public Domain AffectDomain(EntityId id)
	{
		if(AffectedDomains.TryGetValue(id, out var a))
			return a;
			
		a = Mcv.Domains.Find(id, Round.Id);

		if(a == null)
			throw new IntegrityException();
		
		return AffectedDomains[a.Id] = a.Clone();
	}

	public Resource FindResource(EntityId id)
	{
		if(AffectedResources.TryGetValue(id, out var a))
			return a;

		return Mcv.Resources.Find(id, Round.Id);
	}

	public Resource FindResource(Ura address)
	{
		if(AffectedResources.Values.FirstOrDefault(i => i.Address == address) is Resource a && !a.Deleted)
			return a;

		return Mcv.Resources.Find(address, Round.Id);
	}

	public Resource AffectResource(EntityId id)
	{
		if(AffectedResources.TryGetValue(id, out var a))
			return a;
		
		a = Mcv.Resources.Find(id, Round.Id);

		if(a == null)
			throw new IntegrityException();

		return AffectedResources[id] = a.Clone();
	}

  	public Resource AffectResource(Domain domain, string resource)
  	{
		var r =	AffectedResources.Values.FirstOrDefault(i => i.Address.Domain == domain.Address && i.Address.Resource == resource);
		
		if(r != null && !r.Deleted)
			return r;

		r = Mcv.Resources.Find(new Ura(domain.Address, resource), Round.Id);

  		if(r == null)
  		{
  			r = new Resource  {Id = new EntityId(domain.Id.B, GetNextEid(Mcv.Resources, domain.Id.B)),
									Domain = domain.Id,
  									Address = new Ura(domain.Address, resource)};
  		} 
  		else
			r = r.Clone();
    
  		return AffectedResources[r.Id] = r;
  	}
}
