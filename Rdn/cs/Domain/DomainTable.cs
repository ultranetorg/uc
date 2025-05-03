using System.Text;

namespace Uccs.Rdn;

public class DomainTable : Table<AutoId, Domain>
{
	public IEnumerable<RdnRound>	Tail => Mcv.Tail.Cast<RdnRound>();

	public int						KeyToBid(string domain) => EntityId.BytesToBucket(Encoding.UTF8.GetBytes(domain.PadRight(3, '\0'), 0, 3));

	public DomainTable(RdnMcv rds) : base(rds)
	{
	}
	
	public override Domain Create()
	{
		return new Domain(Mcv);
	}
	
 	public Domain Find(string name, int ridmax)
 	{
 		foreach(var r in Tail.Where(i => i.Id <= ridmax))
			if(r.Domains.Affected.Values.FirstOrDefault(i => i.Address == name) is Domain d && !d.Deleted)
				return d;
 		
		var bid = KeyToBid(name);

		return FindBucket(bid)?.Entries.FirstOrDefault(i => i.Address == name);
 	}
}

public class DomainExecution : TableExecution<AutoId, Domain>
{
	new DomainTable Table => base.Table as DomainTable;
		
	public DomainExecution(RdnExecution execution) : base(execution.Mcv.Domains, execution)
	{
	}

	public Domain Find(string name)
	{
		if(Affected.Values.FirstOrDefault(i => i.Address == name) is Domain a)
			return a;

		return Table.Find(name, Execution.Round.Id);
	}

	public Domain Affect(string address)
	{
		if(Affected.Values.FirstOrDefault(i => i.Address == address) is Domain d && !d.Deleted)
			return d;
		
		d = Table.Find(address, Execution.Round.Id);

		if(d != null)
			return Affected[d.Id] = d.Clone() as Domain;
		else
		{
			var b = Table.KeyToBid(address);
			
			int e = Execution.GetNextEid(Table, b);

			d = new Domain(Execution.Mcv) {Id = new AutoId(b, e), Address = address};

			return Affected[d.Id] = d;
		}
	}
}
