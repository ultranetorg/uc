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
	
 	public Domain Find(string name)
 	{
		var bid = KeyToBid(name);

		return FindBucket(bid)?.Entries.FirstOrDefault(i => i.Address == name);
 	}

	public virtual Domain Latest(string name)
	{
		var e = (Mcv.LastConfirmedRound as RdnRound).Domains.Affected.Values.FirstOrDefault(i => i.Address == name);

		if(e != null)
			return e.Deleted ? null : e;

		return Find(name);
	}
}

public class DomainExecution : TableExecution<AutoId, Domain>
{
	new DomainTable										Table => base.Table as DomainTable;
	new RdnExecution									Execution=> base.Execution as RdnExecution;
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
		var e = Affected.Values.FirstOrDefault(i => i.Address == name);

		if(e != null)
			return e.Deleted ? null : e;

		if(Parent != null)
			return (Parent as DomainExecution).Find(name);

		e = Execution.Round.Domains.Affected.Values.FirstOrDefault(i => i.Address == name);

		if(e != null)
			return e.Deleted ? null : e;

		return Table.Find(name);
	}

	public Domain Affect(string name)
	{
		if(Affected.Values.FirstOrDefault(i => i.Address == name) is Domain d)
			return d;

		if(Parent != null)
			d = (Parent as DomainExecution).Find(name);
		else if(Execution.Round.Domains.Affected.Values.FirstOrDefault(i => i.Address == name) is Domain x)
			d = x;
		else
			d = Table.Find(name);

		if(d != null)
			return Affected[d.Id] = d.Clone() as Domain;
		else
		{
			var b = Table.KeyToBid(name);
			
			int e = Execution.GetNextEid(Table, b);

			d = new Domain(Execution.Mcv);
			d.Id = LastCreatedId = new AutoId(b, e);
			d.Address = name;

			return Affected[d.Id] = d;
		}
	}
}
