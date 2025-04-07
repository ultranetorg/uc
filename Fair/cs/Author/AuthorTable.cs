using RocksDbSharp;

namespace Uccs.Fair;

public class AuthorTable : Table<Author>
{
	public new FairMcv	Mcv => base.Mcv as FairMcv;

	public AuthorTable(FairMcv rds) : base(rds)
	{
	}
		
	public override Author Create()
	{
		return new Author(Mcv);
	}

	public override void IndexBucket(WriteBatch batch, Bucket bucket)
	{
		var e = new FairExecution(Mcv, new FairRound(Mcv), null);

		foreach(var i in bucket.Entries.Cast<Author>().Where(i => i.Nickname != ""))
		{
			var w = e.AffectWord(Word.GetId(i.Nickname));

			w.References = [..w.References, new EntityFieldAddress {Entity = i.Id, Field = EntityTextField.AuthorNickname}];
		}
	
		Mcv.Words.Save(batch, e.AffectedWords.Values, null);
	}
}

