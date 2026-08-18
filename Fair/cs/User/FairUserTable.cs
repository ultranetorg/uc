using RocksDbSharp;

namespace Uccs.Fair;

public class FairUserTable : UserTable
{
	public new FairMcv		Mcv => base.Mcv as FairMcv;

	public FairUserTable(Mcv chain) : base(chain)
	{
	}

	public override User Create()
	{
		return new FairUser(Mcv);
	}

	public override User Find(string name)
	{
		var e = Mcv.Names.Find(new StringId(name))?.Entity;

		return e != null && e.Field == EntityTextField.UserName ? Find(e.Id) : null;
	}

	public override void Index(WriteBatch batch, Round lastincommit)
	{
		var e = new FairExecution(Mcv, new FairRound(Mcv), null);

		foreach(var c in Clusters)
			foreach(var b in c.Buckets)
				foreach(var i in b.Entries)
				{
					var w = e.Names.Affect(i.Name);

					w.Entity = new EntityField<EntityTextField>{Id = i.Id, Field = EntityTextField.UserName};
				}
	
		Mcv.Names.Commit(batch, e.Names.Affected.Values, null, lastincommit);
	}
}
