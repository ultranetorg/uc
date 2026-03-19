using System.Diagnostics;

namespace Uccs.Rdn;

public abstract class OutwardOperation : RdnOperation
{	
	public abstract void ConfirmedExecute(Execution execution, Outward task);
}

public class Outward
{
	public OperationId		Id;
	public Time				Expiration;
	public AutoId			Generator;
	public AutoId			User;
	public OutwardOperation	Operation;

	Net.Net					Net;

	public Outward(Net.Net net)
	{
		Net = net;
	}

	public void WriteBaseState(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(User);
		writer.Write(Generator);
		writer.Write(Expiration);
		writer.Write(Net.Constructor.TypeToCode(Operation.GetType())); 
		Operation.Write(writer); 
	}

	public void ReadBaseState(BinaryReader reader)
	{
		Id			= reader.Read<OperationId>();
		User		= reader.Read<AutoId>();
		Generator	= reader.Read<AutoId>();
		Expiration	= reader.Read<Time>();
		Operation	= Net.Constructor.Construct(typeof(Operation), reader.ReadUInt32()) as OutwardOperation;
		Operation.Read(reader); 
	}
}

public class RdnRound : Round
{
	public new RdnMcv						Mcv => base.Mcv as RdnMcv;
	public List<Outward>					Outwards;
	public TableState<AutoId, Domain>		Domains;
	public TableState<AutoId, Resource>		Resources;
	public ForeignResult[]					ConsensusOutwards = {};

	public RdnRound(RdnMcv mcv) : base(mcv)
	{
		Domains		= new (mcv.Domains);
		Resources	= new (mcv.Resources);
	}

	public override Execution CreateExecution(Transaction transaction)
	{
		return new RdnExecution(Mcv, this, transaction);
	}

	public override long UserAllocationFee()
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

	public override void Execute(IEnumerable<Transaction> transactions)
	{
		Outwards	= Id == 0 ? [] : (Previous as RdnRound).Outwards;

		base.Execute(transactions);
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

		ConsensusOutwards	= vs.SelectMany(i => i.Migrations)
								.Distinct()
								.Where(x => Outwards.Any(b => b.Id == x.Id) && vs.Count(b => b.Migrations.Contains(x)) >= gq)
								.Order().ToArray();

		#if IMMISSION
		ConsensusEmissions	= rvs.SelectMany(i => i.Emissions).Distinct()
								 .Where(x => Emissions.Any(e => e.Id == x.OperationId) && rvs.Count(b => b.Emissions.Contains(x)) >= gq)
								 .Order().ToArray();
		#endif
	}

	public override void CopyConfirmed()
	{
		Outwards = Outwards.ToList();
	}

	public override void RegisterForeign(Operation operation, Time time)
	{
		if(operation is OutwardOperation oo)
		{
			Outwards.Add(new Outward(Net)
							 {
								Id			= operation.Id,
								User		= operation.User.Id, 
								Generator	= operation.Transaction.Member,  
								Operation	= oo,
								Expiration	= time + Net.ForeignVerificationDurationLimit
							 });
		}
	}

	public override void ConfirmForeign(Execution execution)
	{
		foreach(var i in ConsensusNnStates)
		{
			var b = Mcv.NnBlocks.Find(j => j.State.Hash.SequenceEqual(i));

			if(b == null)
				throw new ConfirmationException(this, []);

			var d = (execution as RdnExecution).Domains.Affect(b.Net);
			d.NnSelfHash	= b.State.Hash;
			d.NnChildNet	= b.State;

			Mcv.NnBlocks.Remove(b); /// ??????
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

		foreach(var i in ConsensusOutwards)
		{
			var e = Outwards.Find(j => j.Id == i.Id);

			if(i.Approved)
			{
				e.Operation.ConfirmedExecute(execution, e);
				Outwards.Remove(e);
			} 
			else
				execution.AffectUser(e.Generator).AverageUptime -= 10;
		}

		Outwards.RemoveAll(i => i.Expiration < execution.Time);
	}

	public override void WriteGraphState(BinaryWriter writer)
	{
		base.WriteGraphState(writer);

		writer.Write(Candidates, i => i.WriteCandidate(writer));  
		writer.Write(Members, i => i.WriteMember(writer));  
		writer.Write(Outwards, i => i.WriteBaseState(writer));
	}

	public override void ReadGraphState(BinaryReader reader)
	{
		base.ReadGraphState(reader);

		Candidates	= reader.Read<RdnGenerator>(m => m.ReadCandidate(reader)).Cast<Generator>().ToList();
		Members		= reader.Read<RdnGenerator>(m => m.ReadMember(reader)).Cast<Generator>().ToList();
		Outwards	= reader.Read(() => new Outward(Net), i => i.ReadBaseState(reader)).ToList();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		base.WriteConfirmed(writer);

		writer.Write(ConsensusOutwards);
		writer.Write(ConsensusNnStates, writer.Write);
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		base.ReadConfirmed(reader);
		
		ConsensusOutwards	= reader.ReadArray<ForeignResult>();
		ConsensusNnStates = reader.ReadArray(() => reader.ReadBytes(Cryptography.HashLength));
	}
}
