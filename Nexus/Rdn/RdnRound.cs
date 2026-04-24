using System.Diagnostics;

namespace Uccs.Rdn;

public abstract class OutwardOperation : RdnOperation
{	
	public abstract void ConfirmedExecute(RdnExecution execution, Outward task);
}

public class Outward
{
	public int				Id;
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
		writer.Write7BitEncodedInt(Id);
		writer.Write(User);
		writer.Write(Generator);
		writer.Write(Expiration);
		writer.Write(Net.Constructor.TypeToCode(Operation.GetType())); 
		Operation.Write(writer); 
	}

	public void ReadBaseState(BinaryReader reader)
	{
		Id			= reader.Read7BitEncodedInt();
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
	public TableState<AutoId, Domain>		Domains;
	public TableState<AutoId, Resource>		Resources;
	public TableState<AutoId, Subnet>		Subnets;
	public List<Outward>					Outwards;
	public ForeignResult[]					ConsensusOutwards = {};

	public RdnRound(RdnMcv mcv) : base(mcv)
	{
		Domains		= new (mcv.Domains);
		Resources	= new (mcv.Resources);
		Subnets		= new (mcv.Subnets);
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
		if(table == Mcv.Subnets)	return Subnets.Affected;

		return base.AffectedByTable(table);
	}

	public override S FindState<S>(TableBase table)
	{
		if(table == Mcv.Domains)	return Domains as S;
		if(table == Mcv.Resources)	return Resources as S;
		if(table == Mcv.Subnets)	return Subnets as S;

		return base.FindState<S>(table);
	}

	public override void Absorb(Execution execution)
	{
		base.Absorb(execution);

		var e = execution as RdnExecution;

		Domains.Absorb(e.Domains);
		Resources.Absorb(e.Resources);
		Subnets.Absorb(e.Subnets);

		Outwards = e.Outwards;
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

		ConsensusOutwards = vs	.SelectMany(i => i.Migrations)
								.Distinct()
								.Where(x => Outwards.Any(o => o.User == x.User && o.Id == x.Id) && vs.Count(b => b.Migrations.Contains(x)) >= gq)
								.Order().ToArray();
	}

	public override void CopyConfirmed()
	{
		Outwards = Outwards.ToList();
	}

	public override void RegisterForeign(Operation operation, Time time)
	{
		if(operation is OutwardOperation oo)
		{
		}
	}

	public override void ConfirmForeign(Execution execution)
	{
		foreach(var i in ConsensusSubnetTransactions)
		{
			var t = Mcv.SubnetTransactions.Find(j => j.Hash.SequenceEqual(i));

			if(t == null)
				throw new ConfirmationException(this, []);

			var s = (execution as RdnExecution).Subnets.Affect(t.Net);

			if(s.InNonce != t.Nonce + 1)
				throw new ConfirmationException(this, []);

			s.Peers		= t.Peers;	
			s.InNonce	= t.Nonce;

			foreach(var o in t.Operations)
			{
				o.Execute(execution);
			}

			Mcv.SubnetTransactions.Remove(t);
		}

		foreach(var i in ConsensusSubnetTransactionConfirmations)
		{
			var b = Mcv.SubnetTransactionConfirmations.Find(j => j.Hash.SequenceEqual(i));

			if(b == null)
				throw new ConfirmationException(this, []);

			var s = (execution as RdnExecution).Subnets.Affect(b.Net);

			if(!s.OutHash.SequenceEqual(i))
				throw new ConfirmationException(this, []);

			s.OutStatus = OutTransactionStatus.Confirmed;

			Mcv.SubnetTransactionConfirmations.Remove(b);
		}

		foreach(var i in ConsensusOutwards)
		{
			var e = Outwards.Find(j => j.User == i.User && j.Id == i.Id);

			if(i.Approved)
			{
				e.Operation.ConfirmedExecute(execution as RdnExecution, e);
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

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);

		writer.Write(ConsensusOutwards);
		writer.Write(ConsensusSubnetTransactions, writer.Write);
		writer.Write(ConsensusSubnetTransactionConfirmations, writer.Write);
	}

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);
		
		ConsensusOutwards					= reader.ReadArray<ForeignResult>();
		ConsensusSubnetTransactions				= reader.ReadArray(() => reader.ReadBytes(Cryptography.HashLength));
		ConsensusSubnetTransactionConfirmations = reader.ReadArray(() => reader.ReadBytes(Cryptography.HashLength));
	}
}
