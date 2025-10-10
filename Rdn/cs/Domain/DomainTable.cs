using System.Text;

namespace Uccs.Rdn;

public class DomainTable : Table<AutoId, Domain>
{
	public override string			Name => RdnTable.Domain.ToString();
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

	public virtual Domain Latest(string name)
	{
		return Find(name, Mcv.LastConfirmedRound.Id);
	}
}

public class DomainExecution : TableExecution<AutoId, Domain>
{
	new DomainTable										Table => base.Table as DomainTable;
	public static Dictionary<string, HashSet<string>>	Priority = [];
		
	public DomainExecution(RdnExecution execution) : base(execution.Mcv.Domains, execution)
	{
		lock(Priority)
			if(Priority.Count == 0)
			{
				foreach(var tld in Domain.ExclusiveTlds)
				{
					foreach(var i in File.ReadLines(Path.Join(execution.Mcv.Datapath, tld)))
						(Priority.ContainsKey(tld) ? Priority[tld] : (Priority[tld] = [])).Add(i);
				}
			}
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

			d = new Domain(Execution.Mcv);
			d.Id = LastCreatedId = new AutoId(b, e);
			d.Address = address;

			return Affected[d.Id] = d;
		}
	}
}
