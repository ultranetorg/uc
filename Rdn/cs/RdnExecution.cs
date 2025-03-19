namespace Uccs.Rdn;

public class RdnExecution : Execution
{
	public new Rdn				Net => base.Net as Rdn;
	public new RdnMcv			Mcv => base.Mcv as RdnMcv;
	public new RdnRound			Round => base.Round as RdnRound;

	public Dictionary<EntityId, DomainEntry>		AffectedDomains = [];
	public Dictionary<EntityId, ResourceEntry>		AffectedResources = [];

	public RdnExecution(RdnMcv mcv, RdnRound round, Transaction transaction) : base(mcv, round, transaction)
	{
	}

	public override ITableEntry Affect(byte table, EntityId id)
	{
		if(Mcv.Domains.Id == table)		return FindDomain(id) != null ? AffectDomain(id) : null;
		if(Mcv.Resources.Id == table)	return FindResource(id) != null ? AffectResource(id) : null;

		return base.Affect(table, id);
	}

	public DomainEntry FindDomain(EntityId id)
	{
		if(AffectedDomains.TryGetValue(id, out var a))
			return a;

		return Mcv.Domains.Find(id, Round.Id);
	}

	public DomainEntry FindDomain(string name)
	{
		if(AffectedDomains.Values.FirstOrDefault(i => i.Address == name) is DomainEntry a)
			return a;

		return Mcv.Domains.Find(name, Round.Id);
	}

	public DomainEntry AffectDomain(string address)
	{
		if(AffectedDomains.Values.FirstOrDefault(i => i.Address == address) is DomainEntry d && !d.Deleted)
			return d;
		
		d = Mcv.Domains.Find(address, Round.Id);

		if(d != null)
			return AffectedDomains[d.Id] = d.Clone();
		else
		{
			var b = Mcv.Domains.KeyToBid(address);
			
			int e = GetNextEid(Mcv.Domains, b);

			d = new DomainEntry(Mcv) {Id = new EntityId(b, e), Address = address};

			return AffectedDomains[d.Id] = d;
		}
	}

	public DomainEntry AffectDomain(EntityId id)
	{
		if(AffectedDomains.TryGetValue(id, out var a))
			return a;
			
		a = Mcv.Domains.Find(id, Round.Id);

		if(a == null)
			throw new IntegrityException();
		
		return AffectedDomains[a.Id] = a.Clone();
	}

	public ResourceEntry FindResource(EntityId id)
	{
		if(AffectedResources.TryGetValue(id, out var a))
			return a;

		return Mcv.Resources.Find(id, Round.Id);
	}

	public ResourceEntry FindResource(Ura address)
	{
		if(AffectedResources.Values.FirstOrDefault(i => i.Address == address) is ResourceEntry a && !a.Deleted)
			return a;

		return Mcv.Resources.Find(address, Round.Id);
	}

	public ResourceEntry AffectResource(EntityId id)
	{
		if(AffectedResources.TryGetValue(id, out var a))
			return a;
		
		a = Mcv.Resources.Find(id, Round.Id);

		if(a == null)
			throw new IntegrityException();

		return AffectedResources[id] = a.Clone();
	}

  	public ResourceEntry AffectResource(DomainEntry domain, string resource)
  	{
		var r =	AffectedResources.Values.FirstOrDefault(i => i.Address.Domain == domain.Address && i.Address.Resource == resource);
		
		if(r != null && !r.Deleted)
			return r;

		r = Mcv.Resources.Find(new Ura(domain.Address, resource), Round.Id);

  		if(r == null)
  		{
  			r = new ResourceEntry  {Id = new EntityId(domain.Id.B, GetNextEid(Mcv.Resources, domain.Id.B)),
									Domain = domain.Id,
  									Address = new Ura(domain.Address, resource)};
  		} 
  		else
			r = r.Clone();
    
  		return AffectedResources[r.Id] = r;
  	}
}
