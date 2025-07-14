namespace Uccs.Fair;

public abstract class BKTreeTable<E> : Table<RawId, E> where E : BKTerm
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;
	public override bool			IsIndex => true;

	public override abstract E		Create();

	public BKTreeTable(FairMcv mcv) : base(mcv)
	{
	}

	public E Add(string word, Func<RawId, E> find, Func<RawId, E> affect)
	{
		var root = find(new RawId([0]));

		if(root == null)
		{
			var t = affect(Word.GetId(word));
			root = affect(new RawId([0]));
			root.Children[0] = t.Id;
			
			return t;
		}

		var current = find(root.Children[0]);

		while(true)
		{
			byte dist = (byte)ComputeLevenshteinDistance(word, current.Word);

			if(dist == 0)
				return current;

			if(!current.Children.ContainsKey(dist))
			{
				var t = affect(Word.GetId(word));

				current = affect(current.Id);
				current.Children = new SortedDictionary<byte, RawId>(current.Children);
				current.Children[dist] = t.Id;
			
				return t;
			}

			current = find(current.Children[dist]);
		}
	}

	public List<E> Search(string query, int tolerance)
	{
		var result = new List<E>();

		SearchRecursive(Latest(Latest(new RawId([0])).Children[0]), query, tolerance, result);

		return result;
	}

	private void SearchRecursive(E node, string word, int tolerance, List<E> result)
	{
		if(node == null)
			return;

		int dist = ComputeLevenshteinDistance(word, node.Word);

		if(dist <= tolerance)
		{
			result.Add(node);
		}

		for(byte i = (byte)(dist - tolerance); i <= dist + tolerance; i++)
		{
			if(node.Children.ContainsKey(i))
			{
				SearchRecursive(Latest(node.Children[i]), word, tolerance, result);
			}
		}
	}

	public int ComputeLevenshteinDistance(string s, string t)
	{
		var dp = new byte[s.Length + 1, t.Length + 1];

		for(byte i = 0; i <= s.Length; i++) dp[i, 0] = i;
		for(byte j = 0; j <= t.Length; j++) dp[0, j] = j;

		for(int i = 1; i <= s.Length; i++)
		{
			for(int j = 1; j <= t.Length; j++)
			{
				int cost = s[i - 1] == t[j - 1] ? 0 : 1;
				dp[i, j] = (byte)Math.Min(
									Math.Min(dp[i - 1, j] + 1,     // deletion
											 dp[i, j - 1] + 1),    // insertion
											 dp[i - 1, j - 1] + cost); // substitution
			}
		}

		return dp[s.Length, t.Length];
	}

}

//public class EntityTermTable : BKTreeTable<EntityTerm>
//{
//	public EntityTermTable(FairMcv mcv) : base(mcv)
//	{
//	}
//
//	public override EntityTerm Create()
//	{
//		return new EntityTerm(Mcv);
//	}
//}
//
//public class SiteTermTable : BKTreeTable<SiteTerm>
//{
//	public class FoundEntity
//	{
//		public AutoId		Entity;
//		public SiteTerm		Term;
//		public byte			Distance;
//
//		public override string ToString()
//		{
//			return $"{Term.Word}, {Distance}, {Entity}";
//		}
//	}
//
//	public SiteTermTable(FairMcv mcv) : base(mcv)
//	{
//	}
//
//	public override SiteTerm Create()
//	{
//		return new SiteTerm(Mcv);
//	}
//
//	public List<FoundEntity> Search(AutoId site, string term, int tolerance, int skip, int take, IEnumerable<SiteTermTable.FoundEntity> tointersect)
//	{
//		var result = new List<FoundEntity>();
//
//		bool search(SiteTerm node)
//		{
//			if(node == null)
//				return false;
//
//			var dist = ComputeLevenshteinDistance(term, node.Word);
//
//			if(dist <= tolerance && node.References.TryGetValue(site, out var e))
//			{
//				foreach(var i in e)
//				{
//					if(tointersect == null || tointersect.Any(j => j.Entity == i))
//					{
//						if(skip == 0)
//						{
//							result.Add(new FoundEntity {Entity = i, Term = node, Distance = (byte)dist});
//	
//							take--;
//	
//							if(take == 0)
//								return false;
//						}
//						else
//							skip--;
//					}
//				}
//			}
//
//			//foreach(byte i in Enumerable.Range(dist - tolerance, tolerance * 2 + 1).Where(i => i >= 0).OrderBy(i => i))
//			for(int i = Math.Max(0, dist - tolerance); i <= dist + tolerance; i++)
//			{
//				if(node.Children.ContainsKey((byte)i))
//				{
//					if(!search(Latest(node.Children[(byte)i])))
//						return false;
//				}
//			}
//
//			return true;
//		}
//
//		search(Latest(Latest(new RawId([0])).Children[0]) as SiteTerm);
//
//		return result;
//	}
//
//	//public IEnumerable<TextSearchResult> Search(EntityId site, string query, int page, byte lines)
//	//{
//	//	using var r = Mcv.LuceneWriter.GetReader(applyAllDeletes: true);
//	//	var s = new IndexSearcher(r);
//	//	
//	//	var sid = "s" + site;
//	//	
//  	//	var q = new BooleanQuery();
//	//	
//	//	foreach(var i in query.Split(' '))
//	//	{
// 	//		q.Add(new FuzzyQuery(new Term("t", i)), Occur.MUST);
//	//	}
//	//	
// 	//	q.Add(new TermQuery(new Term("e", sid)), Occur.MUST);
//	//	
//	//	var docs = s.Search(q, lines);
//	//	
//	//	if(docs.TotalHits > 0)
//	//		return docs.ScoreDocs.Select(i => new TextSearchResult {Entity = EntityId.Parse(s.Doc(i.Doc).Get("e").Split('\n').Select(i => i.Split(' ')).First(i => i[0] == sid)[1]), 
//	//																Text = s.Doc(i.Doc).Get("t")}).ToArray();
//	//	else
//	//		return null;
//	//}
//
//}

///		Searchs using any part of word no matter the order of letters
///		
/// 	public TextSearchResult[] SearchPublications(EntityId site, string query, int page, int lines)
/// 	{
/// 		IEnumerable<Publication> results = null;
/// 
///  		IEnumerable<Publication> enumerate(Ngram g)
///  		{
///  			var a = g.References.Where(i => i.Field == EntityTextField.PublicationTitle)
///  								.Select(i => Mcv.Publications.Latest(i.Entity))
///  								.Where(p => Mcv.Categories.Latest(p.Category).Site == site);
///  			
///  			foreach(var i in a)
///  				yield return i;
///  
///  			foreach(var i in g.Ngrams.Select(i => Mcv.Ngrams.Latest(i)))
///  				foreach(var k in enumerate(i))
///  					yield return k;
///  		}
/// 
/// 		foreach(var w in query.Split(' '))
/// 		{
/// 			IEnumerable<Publication> word = null;
/// 
/// 			for(int n = w.Length; n >= 1; n--)
/// 			{
/// 				for(int j=0; j <= w.Length - n; j++)
/// 				{
/// 					var id = Ngram.GetId(n, w, j);
/// 	
/// 					var t = Mcv.Ngrams.Latest(id);
/// 		
/// 					if(t != null)
/// 					{
/// 						var a = t.References.Where(i => i.Field == EntityTextField.PublicationTitle)
/// 											.Select(i => Mcv.Publications.Latest(i.Entity))
/// 											.Where(p => Mcv.Categories.Latest(p.Category).Site == site);
/// 	
/// 						if(a.Any())
/// 						{
/// 							if(word == null)
/// 								word = a;
/// 							else
/// 								word = a.Intersect(word, EqualityComparer<Publication>.Create((a, b) => a.Id == b.Id, i => i.Id.GetHashCode()));
/// 						}
/// 					}
/// 				}
/// 
/// 				if(word != null)
/// 					break;
/// 			}
/// 
///  			if(word == null && w.Length <= 4)
///  			{
/// 				var g = Mcv.Ngrams.Latest(Ngram.GetId(w.Length, w, 0));
/// 
///  				if(g != null)
///  				{
///  					word = enumerate(g);
///  				}
///  			}
/// 
/// 			if(results == null)
/// 				results = word;
/// 			else 
/// 			{
/// 				if(word != null)
/// 					results = results.Intersect(word, EqualityComparer<Publication>.Create((a, b) => a.Id == b.Id, i => i.Id.GetHashCode()));
/// 				else
/// 				{
/// 					results = null; /// if no match for current word then nothing is found
/// 					break;
/// 				}
/// 			}
/// 		}
/// 
/// 
/// 
/// 		return results?.Skip(page * lines).Select(p =>	{
/// 															var r = Mcv.Products.Latest(p.Product);
/// 														
/// 															var f = p.Fields.First(i => i.Name == ProductFieldName.Title);
/// 
/// 															return new TextSearchResult {Text = r.GetString(f), Entity = p.Id};
/// 														})
/// 											.Take(lines).ToArray() ?? [];
/// 	}



