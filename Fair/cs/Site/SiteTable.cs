using RocksDbSharp;

namespace Uccs.Fair;

public class SiteTable : Table<Site>
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

	public override void IndexBucket(WriteBatch batch, Bucket bucket)
	{
		var e = new FairExecution(Mcv, new FairRound(Mcv), null);

		foreach(var i in bucket.Entries.Cast<Site>().Where(i => i.Nickname != ""))
		{
			var w = e.AffectWord(Word.GetId(i.Nickname));

			w.References = [..w.References, new EntityFieldAddress {Entity = i.Id, Field = EntityTextField.SiteNickname}];
		}
	
		Mcv.Words.Save(batch, e.AffectedWords.Values, null);
	}
 }
