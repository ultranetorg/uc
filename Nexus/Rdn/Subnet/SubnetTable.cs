using System.Text;

namespace Uccs.Rdn;

public class SubnetTable : Table<AutoId, Subnet>
{
	public override string			Name => RdnTable.Subnet.ToString();

	public int						KeyToBid(string domain) => EntityId.BytesToBucket(Encoding.ASCII.GetBytes(domain.PadRight(3, '\0'), 0, 3));

	public SubnetTable(RdnMcv rds) : base(rds)
	{
	}
	
	public override Subnet Create()
	{
		return new Subnet(Mcv);
	}
	
 	public Subnet Find(string name)
 	{
		var bid = KeyToBid(name);

		return FindBucket(bid)?.Entries.FirstOrDefault(i => i.Address == name);
 	}

	public virtual Subnet Latest(string name)
	{
		var e = (Mcv.LastConfirmedRound as RdnRound).Subnets.Affected.Values.FirstOrDefault(i => i.Address == name);

		if(e != null)
			return e.Deleted ? null : e;

		return Find(name);
	}
}

public class SubnetExecution : TableExecution<AutoId, Subnet>
{
	new SubnetTable			Table => base.Table as SubnetTable;
	new RdnExecution		Execution=> base.Execution as RdnExecution;
		
	public SubnetExecution(RdnExecution execution) : base(execution.Mcv.Subnets, execution)
	{
	}

	public Subnet Find(string name)
	{
		var e = Affected.Values.FirstOrDefault(i => i.Address == name);

		if(e != null)
			return e.Deleted ? null : e;

		if(Parent != null)
			return (Parent as SubnetExecution).Find(name);

		e = Execution.Round.Subnets.Affected.Values.FirstOrDefault(i => i.Address == name);

		if(e != null)
			return e.Deleted ? null : e;

		return Table.Find(name);
	}

	public Subnet Affect(string name)
	{
		if(Affected.Values.FirstOrDefault(i => i.Address == name) is Subnet d)
			return d;

		if(Parent != null)
			d = (Parent as SubnetExecution).Find(name);
		else if(Execution.Round.Subnets.Affected.Values.FirstOrDefault(i => i.Address == name) is Subnet x)
			d = x;
		else
			d = Table.Find(name);

		if(d != null)
			return Affected[d.Id] = d.Clone() as Subnet;
		else
		{
			var b = Table.KeyToBid(name);
			
			int e = Execution.GetNextEid(Table, b);

			d = new Subnet(Execution.Mcv);
			d.Id = LastCreatedId = new AutoId(b, e);
			d.Address = name;

			return Affected[d.Id] = d;
		}
	}
}
