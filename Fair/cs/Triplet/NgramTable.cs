namespace Uccs.Fair;

public class TextSearchResult
{
	public string		Text { get; set; }
	public EntityId		Entity { get; set; }

	public override string ToString()
	{
		return $"{Text}, {Entity}";
	}
}

public class NgramTable : Table<Ngram>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public NgramTable(FairMcv mcv) : base(mcv)
	{
	}
	
	public override Ngram Create()
	{
		return new Ngram(Mcv);
	}

	public TextSearchResult[] SearchPublications(EntityId site, string query, int page, int lines)
	{
		IEnumerable<Publication> results = null;

 		IEnumerable<Publication> enumerate(Ngram g)
 		{
 			var a = g.References.Where(i => i.Field == EntityTextField.PublicationTitle)
 								.Select(i => Mcv.Publications.Latest(i.Entity))
 								.Where(p => Mcv.Categories.Latest(p.Category).Site == site);
 			
 			foreach(var i in a)
 				yield return i;
 
 			foreach(var i in g.Ngrams.Select(i => Mcv.Ngrams.Latest(i)))
 				foreach(var k in enumerate(i))
 					yield return k;
 		}

		foreach(var i in query.Split(' '))
		{
			var w = i;
			
			IEnumerable<Publication> word = null;

			for(int n = w.Length; n >= 1; n--)
			{
				for(int j=0; j <= w.Length - n; j++)
				{
					var id = Ngram.GetId(n, w, j);
	
					var t = Mcv.Ngrams.Latest(id);
		
					if(t != null)
					{
						var a = t.References.Where(i => i.Field == EntityTextField.PublicationTitle)
											.Select(i => Mcv.Publications.Latest(i.Entity))
											.Where(p => Mcv.Categories.Latest(p.Category).Site == site);
	
						if(a.Any())
						{
							if(word == null)
								word = a;
							else
								word = a.Intersect(word, EqualityComparer<Publication>.Create((a, b) => a.Id == b.Id, i => i.Id.GetHashCode()));
						}
					}
				}

				if(word != null)
					break;
			}

 			if(word == null && w.Length <= 4)
 			{
				var g = Mcv.Ngrams.Latest(Ngram.GetId(w.Length, w, 0));

 				if(g != null)
 				{
 					word = enumerate(g);
 				}
 			}

/// search  using partial pattern
/// 
/// 			if(word == null && w.Length <= 4)
/// 			{
/// 				for(int n=w.Length; n>=3; n--)
/// 				{
/// 					foreach(var t in Enumerable.Range(0, w.Length - n + 1).Select(i => Mcv.Ngrams.Latest(Ngram.GetId(n, w, i))).Where(i => i != null))
/// 					{
/// 						var s = enumerate(t);
/// 
/// 						if(s.Any())
/// 						{
/// 							word = s;
/// 							break;
/// 						}
/// 					}
/// 
/// 					if(word != null)
/// 						break;
/// 				}
/// 			}

			if(results == null)
				results = word;
			else 
			{
				if(word != null)
					results = results.Intersect(word, EqualityComparer<Publication>.Create((a, b) => a.Id == b.Id, i => i.Id.GetHashCode()));
				else
					results = null;
			}
		}



		return results?.Skip(page * lines).Select(p =>	{
															var r = Mcv.Products.Latest(p.Product);
														
															var f = p.Fields.First(i => i.Name == ProductField.Title);

															return new TextSearchResult {Text = r.GetString(f), Entity = p.Id};
														})
											.Take(lines).ToArray() ?? [];
	}


 }
