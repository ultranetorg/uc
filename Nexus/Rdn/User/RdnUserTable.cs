using RocksDbSharp;

namespace Uccs.Rdn;

public class RdnUserTable : UserTable
{
	public new RdnMcv		Mcv => base.Mcv as RdnMcv;

	public RdnUserTable(Mcv chain) : base(chain)
	{
	}

	public override User Create()
	{
		return new RdnUser(Mcv);
	}

	public override User Find(string name)
	{
		var id = Mcv.UserNames.Find(new StringId(name))?.Entity;

		return id != null ? Find(id) : null;
	}

	public override void Index(WriteBatch batch, Round lastincommit)
	{
		var e = new RdnExecution(Mcv, new RdnRound(Mcv), null);

		foreach(var c in Clusters)
			foreach(var b in c.Buckets)
				foreach(var i in b.Entries)
				{
					var w = e.UserNames.Affect(new StringId(i.Name));

					w.Entity = i.Id;
				}
	
		Mcv.UserNames.Commit(batch, e.UserNames.Affected.Values, null, lastincommit);
	}
}
