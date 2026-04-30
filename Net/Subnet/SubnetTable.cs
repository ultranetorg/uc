using System.Text;

namespace Uccs.Net;

public class SubnetTable : Table<AutoId, Friend>
{
	public override string			Name => McvTable.Subnet.ToString();

	public int						KeyToBid(string domain) => EntityId.BytesToBucket(Encoding.ASCII.GetBytes(domain.PadRight(3, '\0'), 0, 3));

	public SubnetTable(Mcv rds) : base(rds)
	{
	}
	
	public override Friend Create()
	{
		return new Friend(Mcv);
	}
	
 	public Friend Find(string name)
 	{
		var bid = KeyToBid(name);

		return FindBucket(bid)?.Entries.FirstOrDefault(i => i.Name == name);
 	}

	public virtual Friend Latest(string name)
	{
		var e = Mcv.LastConfirmedRound.Friends.Affected.Values.FirstOrDefault(i => i.Name == name);

		if(e != null)
			return e.Deleted ? null : e;

		return Find(name);
	}
}

public class SubnetExecution : TableExecution<AutoId, Friend>
{
	new SubnetTable			Table => base.Table as SubnetTable;
		
	public SubnetExecution(Execution execution) : base(execution.Mcv.Friends, execution)
	{
	}

	public Friend Find(string name)
	{
		var e = Affected.Values.FirstOrDefault(i => i.Name == name);

		if(e != null)
			return e.Deleted ? null : e;

		if(Parent != null)
			return (Parent as SubnetExecution).Find(name);

		e = Execution.Round.Friends.Affected.Values.FirstOrDefault(i => i.Name == name);

		if(e != null)
			return e.Deleted ? null : e;

		return Table.Find(name);
	}

	public Friend Affect(string name)
	{
		if(Affected.Values.FirstOrDefault(i => i.Name == name) is Friend d)
			return d;

		if(Parent != null)
			d = (Parent as SubnetExecution).Find(name);
		else if(Execution.Round.Friends.Affected.Values.FirstOrDefault(i => i.Name == name) is Friend x)
			d = x;
		else
			d = Table.Find(name);

		if(d != null)
			return Affected[d.Id] = d.Clone() as Friend;
		else
		{
			var b = Table.KeyToBid(name);
			
			int e = Execution.GetNextEid(Table, b);

			d = new Friend(Execution.Mcv);
			d.Id = LastCreatedId = new AutoId(b, e);
			d.Name = name;

			return Affected[d.Id] = d;
		}
	}
}
