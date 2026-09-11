using System.Text;

namespace Uccs.Net;

public class FriendTable : Table<StringId, Friend>
{
	public FriendTable(Mcv rds) : base(rds, McvTable.Friend.ToString())
	{
	}
	
	public override Friend Create()
	{
		return new Friend(Mcv);
	}
	
 	public Friend Find(string name)
 	{
		return Find(new StringId(name));
 	}
	
 	public Friend Latest(string name)
 	{
		return Latest(new StringId(name));
 	}
}

public class FrientExecution : TableExecution<StringId, Friend, FriendTable>
{
	public FrientExecution(Execution execution) : base(execution.Mcv.Friends, execution)
	{
	}

	public Friend Create(string name)
	{
		var f = new Friend(Execution.Mcv);

		f.Id = new StringId(name);

		return Affected[f.Id] = f;
	}

//	public Friend Find(string name)
//	{
//		var e = Affected.Values.FirstOrDefault(i => i.Name == name);
//
//		if(e != null)
//			return e.Deleted ? null : e;
//
//		if(Parent != null)
//			return (Parent as FrientExecution).Find(name);
//
//		e = Execution.Round.Friends.Affected.Values.FirstOrDefault(i => i.Name == name);
//
//		if(e != null)
//			return e.Deleted ? null : e;
//
//		return Table.Find(name);
//	}
//
//	public Friend Affect(string name)
//	{
//		if(Affected.Values.FirstOrDefault(i => i.Name == name) is Friend d)
//			return d;
//
//		if(Parent != null)
//			d = (Parent as FrientExecution).Find(name);
//		else if(Execution.Round.Friends.Affected.Values.FirstOrDefault(i => i.Name == name) is Friend x)
//			d = x;
//		else
//			d = Table.Find(name);
//
//		if(d != null)
//			return Affected[d.Id] = d.Clone() as Friend;
//		else
//		{
//			//var b = Table.KeyToBid(name);
//			//
//			//int e = Execution.GetNextEid(Table, b);
//
//			d = new Friend(Execution.Mcv);
//			d.Id = LastCreatedId = new AutoId(Execution.IncrementMetaInt(MetaEntityType.FriendIdCounter));
//			d.Name = name;
//
//			return Affected[d.Id] = d;
//		}
//	}
}
