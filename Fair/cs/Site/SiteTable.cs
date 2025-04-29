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
					var w = e.AffectWord(Word.GetId(i.Nickname));

					w.References = [..w.References, new EntityFieldAddress {Entity = i.Id, Field = EntityTextField.SiteNickname}];
				}
	
		Mcv.Words.Dissolve(batch, e.AffectedWords.Values, null);
	}
 }
