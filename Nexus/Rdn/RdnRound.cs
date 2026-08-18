using System.Diagnostics;

namespace Uccs.Rdn;

public class RdnRound : Round
{
	public new RdnMcv															Mcv => base.Mcv as RdnMcv;
	public TableState<StringId, DomainName, DomainNameTable>				DomainNames;
	public TableState<AutoId, Domain, DomainTable>								Domains;
	public TableState<AutoId, Resource, ResourceTable>							Resources;
	public TableState<StringId, TextToEntity, ResourceNameIndex>				ResourceNames;
	public TableState<StringId, TextToEntity, NameIndex>						UserNames;

	public RdnRound(RdnMcv mcv) : base(mcv)
	{
		DomainNames	= new (mcv.DomainNames);
		Domains			= new (mcv.Domains);
		Resources		= new (mcv.Resources);
		UserNames		= new (mcv.UserNames);
		ResourceNames	= new (mcv.ResourceNames);
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
		if(table == Mcv.DomainNames)	return DomainNames.Affected;
		if(table == Mcv.Domains)			return Domains.Affected;
		if(table == Mcv.Resources)			return Resources.Affected;
		if(table == Mcv.UserNames)			return UserNames.Affected;
		if(table == Mcv.ResourceNames)		return ResourceNames.Affected;

		return base.AffectedByTable(table);
	}

	public override void ClearAffected()
	{
		base.ClearAffected();
		
		DomainNames.Affected.Clear();
		Domains.Affected.Clear();
		Resources.Affected.Clear();
		UserNames.Affected.Clear();
		ResourceNames.Affected.Clear();
	}

	public override S FindState<S>(TableBase table)
	{
		if(table == Mcv.DomainNames)	return DomainNames as S;
		if(table == Mcv.Domains)			return Domains as S;
		if(table == Mcv.Resources)			return Resources as S;
		if(table == Mcv.UserNames)			return UserNames as S;
		if(table == Mcv.ResourceNames)		return ResourceNames as S;

		return base.FindState<S>(table);
	}

	public override void Absorb(Execution execution)
	{
		base.Absorb(execution);

		var e = execution as RdnExecution;

		DomainNames.Absorb(e.DomainNames);
		Domains.Absorb(e.Domains);
		Resources.Absorb(e.Resources);
		UserNames.Absorb(e.UserNames);
		ResourceNames.Absorb(e.ResourceNames);
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
		writer.Write(Members);  
	}

	public override void ReadGraphState(Reader reader)
	{
		base.ReadGraphState(reader);

		Candidates	= reader.Read<RdnGenerator>(m => m.ReadCandidate(reader)).Cast<Member>().ToList();
		Members		= reader.ReadMany<RdnGenerator>().Cast<Member>().ToList();
	}
}
