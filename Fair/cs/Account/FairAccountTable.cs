using RocksDbSharp;

namespace Uccs.Fair;

public class FairAccountTable : AccountTable
{
	public override string	Name => base.Name.Replace("Fair", null);

	public new FairMcv		Mcv => base.Mcv as FairMcv;

	public FairAccountTable(Mcv chain) : base(chain)
	{
	}

	public override Account Create()
	{
		return new FairAccount(Mcv);
	}

	public override void Index(WriteBatch batch, Round lastincommit)
	{
		var e = new FairExecution(Mcv, new FairRound(Mcv), null);

		foreach(var cl in Clusters)
			foreach(var b in cl.Buckets)
				foreach(var i in b.Entries.Cast<FairAccount>().Where(i => i.Nickname != ""))
				{
					var w = e.Words.Affect(Word.GetId(i.Nickname));

					w.References = [..w.References, new EntityFieldAddress {Entity = i.Id, Field = EntityTextField.AccountNickname}];
				}
	
		Mcv.Words.Commit(batch, e.Words.Affected.Values, e.Words, null);
	}
}
