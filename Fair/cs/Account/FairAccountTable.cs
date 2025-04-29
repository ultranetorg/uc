using RocksDbSharp;

namespace Uccs.Fair;

public class FairAccountTable : AccountTable
{
	public new FairMcv		Mcv => base.Mcv as FairMcv;

	public FairAccountTable(Mcv chain) : base(chain)
	{
	}

	public override Account Create()
	{
		return new FairAccount(Mcv);
	}

	public override void Index(WriteBatch batch)
	{
		var e = new FairExecution(Mcv, new FairRound(Mcv), null);

		foreach(var cl in Clusters)
			foreach(var b in cl.Buckets)
				foreach(var i in b.Entries.Cast<FairAccount>().Where(i => i.Nickname != ""))
				{
					var w = e.AffectWord(Word.GetId(i.Nickname));

					w.References = [..w.References, new EntityFieldAddress {Entity = i.Id, Field = EntityTextField.AccountNickname}];
				}
	
		Mcv.Words.Dissolve(batch, e.AffectedWords.Values, null);
	}
}
