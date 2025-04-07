using Lucene.Net.Index;
using Lucene.Net.Search;

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

	public IEnumerable<TextSearchResult> Search(EntityId site, string query, int page, byte lines)
	{
		using var r = Mcv.LuceneWriter.GetReader(applyAllDeletes: true);
		var s = new IndexSearcher(r);

		var sid = "s" + site;

  		var q = new BooleanQuery();

		foreach(var i in query.Split(' '))
		{
 			q.Add(new FuzzyQuery(new Term("t", i)), Occur.MUST);
		}

 		q.Add(new TermQuery(new Term("e", sid)), Occur.MUST);

		var docs = s.Search(q, lines);

		if(docs.TotalHits > 0)
			return docs.ScoreDocs.Select(i => new TextSearchResult {Entity = EntityId.Parse(s.Doc(i.Doc).Get("e").Split('\n').Select(i => i.Split(' ')).First(i => i[0] == sid)[1]), 
																	Text = s.Doc(i.Doc).Get("t")}).ToArray();
		else
			return null;
	}
}
