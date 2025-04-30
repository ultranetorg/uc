using RocksDbSharp;

namespace Uccs.Fair;

public class SiteTable : Table<AutoId, Site>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public SiteTable(FairMcv rds) : base(rds)
	{
	}
	
	public override Site Create()
	{
		return new Site(Mcv);
	}

	public override void Index(WriteBatch batch)
	{
		var e = new FairExecution(Mcv, new FairRound(Mcv), null);

		foreach(var cl in Clusters)
			foreach(var b in cl.Buckets)
				foreach(var i in b.Entries.Where(i => i.Nickname != ""))
				{
					var w = e.Words.Affect(Word.GetId(i.Nickname));

					w.References = [..w.References, new EntityFieldAddress {Entity = i.Id, Field = EntityTextField.SiteNickname}];
				}
	
		Mcv.Words.Commit(batch, e.Words.Affected.Values, null, null);
	}
}

public class SiteExecution : TableExecution<AutoId, Site>
{
	public SiteExecution(FairExecution execution) : base(execution.Mcv.Sites, execution)
	{
	}

	public Site Create(Account signer)
	{
		var b = Execution.Mcv.Accounts.KeyToBucket(signer.Address);
		
		int e = Execution.GetNextEid(Table, b);

		var a = Table.Create();
		
		a.Id = new AutoId(b, e);
		a.Categories = [];
		a.Moderators = [];
		a.Authors = [];
		a.Disputes = [];
		a.ChangePolicies = [];
		a.Nickname = "";
		a.Description = "";
		
		return Affected[a.Id] = a;
	}
}
