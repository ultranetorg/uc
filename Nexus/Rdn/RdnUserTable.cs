using RocksDbSharp;

namespace Uccs.Rdn;

public class RdnUserTable : UserTable
{
	public new RdnMcv		Mcv => base.Mcv as RdnMcv;

	public RdnUserTable(Mcv chain) : base(chain)
	{
	}

	public override User Find(string name)
	{
		var id = Mcv.Names.Find(NameIndex.GetId(name))?.Entities.Find(i => i.Field == EntityTextField.UserName)?.Id;

		return id != null ? Find(id) : null;
	}

	public override void Index(WriteBatch batch, Round lastincommit)
	{
		var e = new RdnExecution(Mcv, new RdnRound(Mcv), null);

		foreach(var c in Clusters)
			foreach(var b in c.Buckets)
				foreach(var i in b.Entries)
				{
					var w = e.Names.Affect(NameIndex.GetId(i.Name));

					w.Entities = w.Entities.Add(new EntityField<EntityTextField> {Id = i.Id, Field = EntityTextField.UserName});
				}
	
		Mcv.Names.Commit(batch, e.Names.Affected.Values, null, lastincommit);
	}
}
