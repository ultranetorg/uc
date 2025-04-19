using RocksDbSharp;

namespace Uccs.Fair;

public class PublicationTable : Table<Publication>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public PublicationTable(FairMcv rds) : base(rds)
	{
	}
	
	public override Publication Create()
	{
		return new Publication(Mcv);
	}

	public override void IndexBucket(WriteBatch batch, Bucket bucket)
	{
		var e = new FairExecution(Mcv, new FairRound(Mcv), null);

		foreach(var i in bucket.Entries.Cast<Publication>())
		{
			var c = e.FindCategory(i.Category);
			var r = Mcv.Products.Find(i.Product);
			var f = i.Fields.FirstOrDefault(f => f.Name == ProductField.Title);

			if(f != null)
			{
				var t = r.Get(f);

				var w = e.AffectPublicationTitle(new RawId(t));
	
				w.References[c.Site] = [..w.References[c.Site], i.Id];
			}
		}
	
		Mcv.Words.Save(batch, e.AffectedWords.Values, null);
	}

	//public IEnumerable<TextSearchResult> Search(EntityId site, string query, int page, byte lines)
	//{
	//	using var r = Mcv.LuceneWriter.GetReader(applyAllDeletes: true);
	//	var s = new IndexSearcher(r);
	//	
	//	var sid = "s" + site;
	//	
  	//	var q = new BooleanQuery();
	//	
	//	foreach(var i in query.Split(' '))
	//	{
 	//		q.Add(new FuzzyQuery(new Term("t", i)), Occur.MUST);
	//	}
	//	
 	//	q.Add(new TermQuery(new Term("e", sid)), Occur.MUST);
	//	
	//	var docs = s.Search(q, lines);
	//	
	//	if(docs.TotalHits > 0)
	//		return docs.ScoreDocs.Select(i => new TextSearchResult {Entity = EntityId.Parse(s.Doc(i.Doc).Get("e").Split('\n').Select(i => i.Split(' ')).First(i => i[0] == sid)[1]), 
	//																Text = s.Doc(i.Doc).Get("t")}).ToArray();
	//	else
	//		return null;
	//}
}
