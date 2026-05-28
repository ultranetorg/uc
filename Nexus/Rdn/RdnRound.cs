using System.Diagnostics;

namespace Uccs.Rdn;

public class RdnRound : Round
{
	public new RdnMcv						Mcv => base.Mcv as RdnMcv;
	public TableState<AutoId, Domain>		Domains;
	public TableState<AutoId, Resource>		Resources;

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

	public override void FinishExecution()
	{
		foreach(var r in Resources.Affected.Values)
		{
			if(r.Outbounds != null)
				foreach(var l in r.Outbounds.Where(i => i.Affected))
					l.Affected = false;
		}
	}

	public override void WriteGraphState(Writer writer)
	{
		base.WriteGraphState(writer);

		writer.Write(Candidates, i => i.WriteCandidate(writer));  
		writer.Write(Members, i => i.WriteMember(writer));  
	}

	public override void ReadGraphState(Reader reader)
	{
		base.ReadGraphState(reader);

		Candidates	= reader.Read<RdnGenerator>(m => m.ReadCandidate(reader)).Cast<Generator>().ToList();
		Members		= reader.Read<RdnGenerator>(m => m.ReadMember(reader)).Cast<Generator>().ToList();
	}
}
