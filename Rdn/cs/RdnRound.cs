using System.Diagnostics;

namespace Uccs.Rdn;

public class RdnRound : Round
{
	public new RdnMcv								Mcv => base.Mcv as RdnMcv;
	public List<DomainMigration>					Migrations;
	public Dictionary<EntityId, DomainEntry>		AffectedDomains = [];
	public Dictionary<EntityId, ResourceEntry>		AffectedResources = [];
	public Dictionary<int, int>						NextDomainEids = [];
	public Dictionary<int, int>						NextResourceEids = [];
	public ForeignResult[]							ConsensusMigrations = {};

	public RdnRound(RdnMcv rds) : base(rds)
	{
		NextEids = Mcv.Tables.Select(i => new Dictionary<int, int>()).ToArray();
	}

	public override long AccountAllocationFee(Account account)
	{
		return Operation.ToBD(Mcv.Net.EntityLength, Uccs.Net.Mcv.Forever);
	}

	public override System.Collections.IDictionary AffectedByTable(TableBase table)
	{
		if(table == Mcv.Domains)	return AffectedDomains;
		if(table == Mcv.Resources)	return AffectedResources;

		return base.AffectedByTable(table);
	}

	public DomainEntry AffectDomain(string address)
	{
		if(AffectedDomains.Values.FirstOrDefault(i => i.Address == address) is DomainEntry d && !d.Deleted)
			return d;
		
		d = Mcv.Domains.Find(address, Id - 1);

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
			
		a = Mcv.Domains.Find(id, Id - 1);

		if(a == null)
			throw new IntegrityException();
		
		return AffectedDomains[a.Id] = a.Clone();
	}

	public ResourceEntry AffectResource(EntityId id)
	{
		if(AffectedResources.TryGetValue(id, out var a))
			return a;
		
		a = Mcv.Resources.Find(id, Id - 1);

		if(a == null)
			throw new IntegrityException();

		return AffectedResources[id] = a.Clone();
	}

  	public ResourceEntry AffectResource(DomainEntry domain, string resource)
  	{
		var r =	AffectedResources.Values.FirstOrDefault(i => i.Address.Domain == domain.Address && i.Address.Resource == resource);
		
		if(r != null && !r.Deleted)
			return r;

		r = Mcv.Resources.Find(new Ura(domain.Address, resource), Id - 1);

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

	public override void RestartExecution()
	{
		Migrations	= Id == 0 ? new() : (Previous as RdnRound).Migrations;
	}

	public override void FinishExecution()
	{
		foreach(var r in AffectedResources.Values)
		{
			if(r.Outbounds != null)
				foreach(var l in r.Outbounds.Where(i => i.Affected))
					l.Affected = false;
		}
	}

	public override void Elect(Vote[] votes, int gq)
	{
		var vs = votes.Cast<RdnVote>();

		ConsensusMigrations	= vs.SelectMany(i => i.Migrations)
								.Distinct()
								.Where(x => Migrations.Any(b => b.Id == x.OperationId) && vs.Count(b => b.Migrations.Contains(x)) >= gq)
								.Order().ToArray();

		#if IMMISSION
		ConsensusEmissions	= rvs.SelectMany(i => i.Emissions).Distinct()
								 .Where(x => Emissions.Any(e => e.Id == x.OperationId) && rvs.Count(b => b.Emissions.Contains(x)) >= gq)
								 .Order().ToArray();
		#endif
	}

	public override void CopyConfirmed()
	{
		Migrations = Migrations.ToList();
	}

	public override void RegisterForeign(Operation o)
	{
		if(o is DomainMigration m)
		{
			m.Generator = m.Transaction.Member;
			Migrations.Add(m);
		}
	}

	public override void ConfirmForeign()
	{
		foreach(var i in ConsensusNtnStates)
		{
			var b = Mcv.NtnBlocks.Find(j => j.State.Hash.SequenceEqual(i));

			if(b == null)
				throw new ConfirmationException(this, []);

			var d = AffectDomain(b.Net);
			d.NtnSelfHash	= b.State.Hash;
			d.NtnChildNet	= b.State;

			Mcv.NtnBlocks.Remove(b);
		}

		#if IMMISSION
		foreach(var i in ConsensusEmissions)
		{
			var e = Emissions.Find(j => j.Id == i.OperationId);

			if(i.Approved)
			{
				e.ConfirmedExecute(this);
				Emissions.Remove(e);
			} 
			else
				AffectAccount(Mcv.Accounts.Find(e.Generator, Id).Address).AvarageUptime -= 10;
		}

		Emissions.RemoveAll(i => Id > i.Id.Ri + Mcv.Net.ExternalVerificationDurationLimit);
		#endif

		foreach(var i in ConsensusMigrations)
		{
			var e = Migrations.Find(j => j.Id == i.OperationId);

			if(i.Approved)
			{
				e.ConfirmedExecute(this);
				Migrations.Remove(e);
			} 
			else
				AffectAccount(e.Generator).AverageUptime -= 10;
		}

		Migrations.RemoveAll(i => Id > i.Id.Ri + Mcv.Net.ExternalVerificationRoundDurationLimit);
	}

	public override void WriteBaseState(BinaryWriter writer)
	{
		base.WriteBaseState(writer);

		writer.Write(Candidates, i => i.WriteCandidate(writer));  
		writer.Write(Members, i => i.WriteMember(writer));  
		writer.Write(Migrations, i => i.WriteBaseState(writer));
	}

	public override void ReadBaseState(BinaryReader reader)
	{
		base.ReadBaseState(reader);

		Candidates	= reader.Read<RdnGenerator>(m => m.ReadCandidate(reader)).Cast<Generator>().ToList();
		Members		= reader.Read<RdnGenerator>(m => m.ReadMember(reader)).Cast<Generator>().ToList();
		Migrations	= reader.Read<DomainMigration>(m => m.ReadBaseState(reader)).ToList();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		base.WriteConfirmed(writer);

		#if IMMISSION
		writer.Write(ConsensusEmissions);
		#endif
		writer.Write(ConsensusMigrations);
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		base.ReadConfirmed(reader);
		
		#if IMMISSION
		ConsensusEmissions	= reader.ReadArray<ForeignResult>();
		#endif
		ConsensusMigrations	= reader.ReadArray<ForeignResult>();
	}
}
