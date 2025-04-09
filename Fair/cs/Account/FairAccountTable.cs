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

	public override void IndexBucket(WriteBatch batch, Bucket bucket)
	{
		var e = new FairExecution(Mcv, new FairRound(Mcv), null);

		foreach(var i in bucket.Entries.Cast<FairAccount>().Where(i => i.Nickname != ""))
		{
			var w = e.AffectWord(Word.GetId(i.Nickname));

			w.References = [..w.References, new EntityFieldAddress {Entity = i.Id, Field = EntityTextField.AccountNickname}];
		}
	
		Mcv.Words.Save(batch, e.AffectedWords.Values, null);
	}
}
