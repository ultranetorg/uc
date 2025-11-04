using System.Diagnostics;

namespace Uccs.Rdn;

public class RdnRound : Round
{
	public new RdnMcv						Mcv => base.Mcv as RdnMcv;
	public List<DomainMigration>			Migrations;
	public TableState<AutoId, Domain>		Domains;
	public TableState<AutoId, Resource>		Resources;
	public ForeignResult[]					ConsensusMigrations = {};

	public RdnRound(RdnMcv mcv) : base(mcv)
	{
		Domains		= new (mcv.Domains);
		Resources	= new (mcv.Resources);
	}

	public override Execution CreateExecution(Transaction transaction)
	{
		return new RdnExecution(Mcv, this, transaction);
	}

	public override long AccountAllocationFee(Account account)
	{
		return Execution.ToBD(Mcv.Net.EntityLength, Uccs.Net.Mcv.Forever);
	}

	public override System.Collections.IDictionary AffectedByTable(TableBase table)
	{
		if(table == Mcv.Domains)	return Domains.Affected;
		if(table == Mcv.Resources)	return Resources.Affected;

		return base.AffectedByTable(table);
	}

	public override S FindState<S>(TableBase table)
	{
		if(table == Mcv.Domains)	return Domains as S;
		if(table == Mcv.Resources)	return Resources as S;

		return base.FindState<S>(table);
	}

	public override void Absorb(Execution execution)
	{
		base.Absorb(execution);

		var e = execution as RdnExecution;

		Domains.Absorb(e.Domains);
		Resources.Absorb(e.Resources);
	}

	public override void Execute(IEnumerable<Transaction> transactions, bool trying = false)
	{
		Migrations	= Id == 0 ? [] : (Previous as RdnRound).Migrations;

		base.Execute(transactions, trying);
	}

	public override void FinishExecution()
	{
		foreach(var r in Resources.Affected.Values)
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

			var d = (execution as RdnExecution).Domains.Affect(b.Net);
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

	public override void WriteGraphState(BinaryWriter writer)
	{
		base.WriteGraphState(writer);

		writer.Write(Candidates, i => i.WriteCandidate(writer));  
		writer.Write(Members, i => i.WriteMember(writer));  
		writer.Write(Migrations, i => i.WriteBaseState(writer));
	}

	public override void ReadGraphState(BinaryReader reader)
	{
		base.ReadGraphState(reader);

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
