using System.Diagnostics;

namespace Uccs.Rdn;

public class RdnRound : Round
{
	public new RdnMcv								Mcv => base.Mcv as RdnMcv;
	public List<DomainMigration>					Migrations;
	public Dictionary<EntityId, Domain>		AffectedDomains = [];
	public Dictionary<EntityId, Resource>		AffectedResources = [];
	public ForeignResult[]							ConsensusMigrations = {};

	public RdnRound(RdnMcv rds) : base(rds)
	{
	}

	public override void Consume(Execution execution)
	{
		base.Consume(execution);

		var e = execution as RdnExecution;

		foreach(var i in e.AffectedDomains)
			AffectedDomains[i.Key] = i.Value;

		foreach(var i in e.AffectedResources)
			AffectedResources[i.Key] = i.Value;
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

	public override Execution CreateExecution(Transaction transaction)
	{
		return new RdnExecution(Mcv, this, transaction);
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

	public override void ConfirmForeign(Execution execution)
	{
		foreach(var i in ConsensusNtnStates)
		{
			var b = Mcv.NtnBlocks.Find(j => j.State.Hash.SequenceEqual(i));

			if(b == null)
				throw new ConfirmationException(this, []);

			var d = (execution as RdnExecution).AffectDomain(b.Net);
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
				e.ConfirmedExecute(execution);
				Migrations.Remove(e);
			} 
			else
				execution.AffectAccount(e.Generator).AverageUptime -= 10;
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
