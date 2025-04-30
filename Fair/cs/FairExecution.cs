using System.Text;

namespace Uccs.Fair;

public class FairExecution : Execution
{
	public new Fair				Net => base.Net as Fair;
	public new FairMcv			Mcv => base.Mcv as FairMcv;
	public new FairRound		Round => base.Round as FairRound;

	public Dictionary<AutoId, Author>					AffectedAuthors = new();
	public Dictionary<AutoId, Product>					AffectedProducts = new();
	public Dictionary<AutoId, Site>						AffectedSites = new();
	public Dictionary<AutoId, Category>					AffectedCategories = new();
	public Dictionary<AutoId, Publication>				AffectedPublications = new();
	public Dictionary<AutoId, Review>					AffectedReviews = new();
	public Dictionary<AutoId, Dispute>					AffectedDisputes = new();
	public Dictionary<RawId, Word>						AffectedWords = new();
	public PublicationTitleExecution				PublicationTitles;

	public FairExecution(FairMcv mcv, FairRound round, Transaction transaction) : base(mcv, round, transaction)
	{
		PublicationTitles = new(this);
	}

	public override ITableEntry Affect(byte table, EntityId id)
	{
		if(table == Mcv.Authors.Id)				return FindAuthor(id as AutoId)				!= null	? AffectAuthor(id as AutoId) : null;
		if(table == Mcv.Products.Id)			return FindProduct(id as AutoId)				!= null	? AffectProduct(id as AutoId) : null;
		if(table == Mcv.Sites.Id)				return FindSite(id as AutoId)					!= null	? AffectSite(id as AutoId) : null;
		if(table == Mcv.Categories.Id)			return FindCategory(id as AutoId)				!= null	? AffectCategory(id as AutoId) : null;
		if(table == Mcv.Publications.Id)		return FindPublication(id as AutoId)			!= null	? AffectPublication(id as AutoId) : null;
		if(table == Mcv.Reviews.Id)				return FindReview(id as AutoId)				!= null	? AffectReview(id as AutoId) : null;
		if(table == Mcv.Disputes.Id)			return FindDispute(id as AutoId)				!= null	? AffectDispute(id as AutoId) : null;
		//if(table == Mcv.Words.Id)				return FindWord(id as RawId)					!= null	? AffectWord(id as RawId) : null;
		//if(table == Mcv.PublicationTitles.Id)	return Mcv.PublicationTitles.Find(id as HnswId)	!= null	? AffectPublicationTitle(id as RawId) : null;

		return base.Affect(table, id);
	}

	public override System.Collections.IDictionary AffectedByTable(TableBase table)
	{
		if(table == Mcv.Authors)			return AffectedAuthors;
		if(table == Mcv.Products)			return AffectedProducts;
		if(table == Mcv.Sites)				return AffectedSites;
		if(table == Mcv.Categories)			return AffectedCategories;
		if(table == Mcv.Publications)		return AffectedPublications;
		if(table == Mcv.Reviews)			return AffectedReviews;
		if(table == Mcv.Disputes)			return AffectedDisputes;
		if(table == Mcv.Words)				return AffectedWords;
		if(table == Mcv.PublicationTitles)	return PublicationTitles.Affected;

		return base.AffectedByTable(table);
	}

	public new FairAccount FindAccount(AutoId id)
	{
		return base.FindAccount(id) as FairAccount;
	}

	public new FairAccount AffectAccount(AutoId id)
	{
		return base.AffectAccount(id) as FairAccount;
	}

	public override Account AffectSigner()
	{
		if(Transaction.Operations.All(i => i.NonExistingSignerAllowed))
		{
			if(AffectedAccounts.FirstOrDefault(i => i.Value.Address == Transaction.Signer).Value is Account a)
				return a;
		
			a = Mcv.Accounts.Find(Transaction.Signer, Round.Id)?.Clone();	

			if(a != null)
				TransferEnergyIfNeeded(a);
			else
				a = CreateAccount(Transaction.Signer);

			AffectedAccounts[a.Id] = a;

			return a;
		}

		return base.AffectSigner();
	}

	public override FairAccount CreateAccount(AccountAddress address)
	{
		var a = base.CreateAccount(address) as FairAccount;

		a.Reviews = [];
		a.Sites = [];
		a.Authors = [];
		a.FavoriteSites = [];
		a.Nickname = "";

		return a;
	}

	public void DeleteAccount(FairAccount account)
	{
		account.Deleted = true;
// 
// 		foreach(var i in account.Authors.Select(i => Mcv.Authors.Find(i, Id)))
// 		{
// 			if(i.Owners.Length == 1)
// 				DeleteAuthor(i);
// 		}
// 
// 		foreach(var i in account.Sites.Select(i => Mcv.Sites.Find(i, Id)))
// 		{
// 			if(i.Moderators.Length == 1)
// 				DeleteAuthor(i);
// 		}
	}

 	public Author FindAuthor(AutoId id)
 	{ 
 		if(AffectedAuthors.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.Authors.Find(id, Round.Id);
 	}

	public Author CreateAuthor(AccountAddress signer)
	{
		var b = Mcv.Accounts.KeyToBucket(signer);
		
		int e = GetNextEid(Mcv.Authors, b);

		var a = Mcv.Authors.Create();
		a.Id = new AutoId(b, e);
		a.Products = [];
		a.Owners = [];
		a.Sites = [];
		a.Nickname = "";			

		return AffectedAuthors[a.Id] = a;
	}

	public Author AffectAuthor(AutoId id)
	{
		if(AffectedAuthors.TryGetValue(id, out var a))
			return a;
			
		var e = FindAuthor(id);

		e = AffectedAuthors[id] = e.Clone();

		TransferEnergyIfNeeded(e);

		return e;
	}

	private void DeleteAuthor(Author author)
	{
	}
 
 	public Product FindProduct(AutoId id)
 	{
 		if(AffectedProducts.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.Products.Find(id, Round.Id);
 	}

	public Product CreateProduct(Author author)
	{
		int e = GetNextEid(Mcv.Products, author.Id.B);

  		var	p = new Product();

		p.Id = new AutoId(author.Id.B, e);
		p.Fields = []; 
		p.Publications = [];

  		return AffectedProducts[p.Id] = p;
	}

	public Product AffectProduct(AutoId id)
	{
		if(AffectedProducts.TryGetValue(id, out var a))
			return a;
		
		a = Mcv.Products.Find(id, Round.Id);

		return AffectedProducts[id] = a.Clone();
	}

 	public Site FindSite(AutoId id)
 	{
 		if(AffectedSites.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.Sites.Find(id, Round.Id);
 	}

	public Site CreateSite(Account signer)
	{
		var b = Mcv.Accounts.KeyToBucket(signer.Address);
		
		int e = GetNextEid(Mcv.Sites, b);

		var a = Mcv.Sites.Create();
		
		a.Id = new AutoId(b, e);
		a.Categories = [];
		a.Moderators = [];
		a.Authors = [];
		a.Disputes = [];
		a.ChangePolicies = [];
		a.Nickname = "";
		a.Description = "";
		
		return AffectedSites[a.Id] = a;
	}

	public Site AffectSite(AutoId id)
	{
		if(AffectedSites.TryGetValue(id, out var a))
			return a;
			
		var e = Mcv.Sites.Find(id, Round.Id);

		return AffectedSites[id] = e.Clone();
	}
 
 	public Category FindCategory(AutoId id)
 	{
 		if(AffectedCategories.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.Categories.Find(id, Round.Id);
 	}

	public Category CreateCategory(Site site)
	{
		int e = GetNextEid(Mcv.Categories, site.Id.B);

		var a = Mcv.Categories.Create();
		a.Id = new AutoId(site.Id.B, e);
		a.Categories = [];
		a.Publications = [];

		return AffectedCategories[a.Id] = a;
	}
		
	public Category AffectCategory(AutoId id)
	{
		if(AffectedCategories.TryGetValue(id, out var a))
			return a;
			
		var e = Mcv.Categories.Find(id, Round.Id);

		return AffectedCategories[id] = e.Clone();
	}
 
 	public Publication FindPublication(AutoId id)
 	{
 		if(AffectedPublications.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.Publications.Find(id, Round.Id);
 	}

	public Publication CreatePublication(Site site)
	{
		int e = GetNextEid(Mcv.Publications, site.Id.B);

		var a = Mcv.Publications.Create();
		a.Id = new AutoId(site.Id.B, e);
		a.Fields = [];
		a.Changes = [];
		a.Reviews = [];
		a.ReviewChanges = [];
			
		return AffectedPublications[a.Id] = a;
	}

	public Publication AffectPublication(AutoId id)
	{
		if(AffectedPublications.TryGetValue(id, out var a))
			return a;
			
		var e = Mcv.Publications.Find(id, Round.Id);

		return AffectedPublications[id] = e.Clone();
	}
 
 	public Dispute FindDispute(AutoId id)
 	{
 		if(AffectedDisputes.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.Disputes.Find(id, Round.Id);
 	}

	public Review CreateReview(Publication publication)
	{
		int e = GetNextEid(Mcv.Reviews, publication.Id.B);

		var a = Mcv.Reviews.Create();
		a.Id = new AutoId(publication.Id.B, e);

		return AffectedReviews[a.Id] = a;
	}

	public Review AffectReview(AutoId id)
	{
		if(AffectedReviews.TryGetValue(id, out var a))
			return a;
			
		var e = Mcv.Reviews.Find(id, Round.Id);

		return AffectedReviews[id] = e.Clone();
	}

 	public Review FindReview(AutoId id)
 	{
 		if(AffectedReviews.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.Reviews.Find(id, Round.Id);
 	}

	public Dispute CreateDispute(Site site)
	{
		int e = GetNextEid(Mcv.Disputes, site.Id.B);

		var a = Mcv.Disputes.Create();
		a.Id = new AutoId(site.Id.B, e);
		a.Yes = [];
		a.No = [];
		a.Abs = [];

		return AffectedDisputes[a.Id] = a;
	}

	public Dispute AffectDispute(AutoId id)
	{
		if(AffectedDisputes.TryGetValue(id, out var a))
			return a;
			
		var e = Mcv.Disputes.Find(id, Round.Id);

		return AffectedDisputes[id] = e.Clone();
	}

	//public Ngram CreateNgram(RawId id)
	//{
	//	var a = Mcv.Ngrams.Create();
	//	a.Id = id;
	//	a.References = [];
	//
	//	return AffectedNgrams[a.Id] = a;
	//}
	
//  public Ngram FindNgram(RawId id)
//  {
//  	if(AffectedNgrams.TryGetValue(id, out var a))
//  		return a;
//  
//  	return Mcv.Ngrams.Find(id, Round.Id);
//  }
// 
// 	//public Ngram CreateNgram(RawId id)
// 	//{
// 	//	var a = Mcv.Ngrams.Create();
// 	//	a.Id = id;
// 	//	a.References = [];
// 	//
// 	//	return AffectedNgrams[a.Id] = a;
// 	//}
// 
// 	public Ngram AffectNgram(RawId id)
// 	{
// 		if(AffectedNgrams.TryGetValue(id, out var a))
// 			return a;
// 			
// 		a = Mcv.Ngrams.Find(id, Round.Id);
// 
// 		if(a == null)
// 		{
// 			a = Mcv.Ngrams.Create();
// 			a.Id = id;
// 			a.References = [];
// 			a.Ngrams = [];
// 		
// 			return AffectedNgrams[id] = a;
// 		} 
// 		else
// 		{
// 			return AffectedNgrams[id] = a.Clone();
// 		}
// 	}
 
 	public Word FindWord(RawId id)
 	{
 		if(AffectedWords.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.Words.Find(id, Round.Id);
 	}

	public Word AffectWord(RawId id)
	{
		if(AffectedWords.TryGetValue(id, out var a))
			return a;
			
		a = Mcv.Words.Find(id, Round.Id);

		if(a == null)
		{
			a = Mcv.Words.Create();
			a.Id = id;
			a.References = [];
		
			return AffectedWords[id] = a;
		} 
		else
		{
			return AffectedWords[id] = a.Clone();
		}
	}

	public void RegisterWord(string word, EntityTextField field, AutoId entity)
	{
		var id = Word.GetId(word);
		var w = FindWord(id)?.References.FirstOrDefault(e => e.Field == field && e.Entity == entity);;
		
		if(w == null)
		{
			var t = AffectWord(id);
	
			t.References = [..t.References, new EntityFieldAddress {Entity = entity, Field = field}];
		}
	}

	public void UnregisterWord(string word, EntityTextField field, AutoId entity)
	{
		var id = Word.GetId(word);
		var w = FindWord(id)?.References.FirstOrDefault(e => e.Field == field && e.Entity == entity);;
		
		if(w == null)
		{
			var t = AffectWord(id);
	
			t.References = t.References.Remove(w);
		}
	}

/// 	public void IndexText(string text, EntityTextField field, EntityId entity, EntityId site)
/// 	{
/// 		var id = new EntityFieldAddress(entity, field);
/// 
///  		if(AffectedTexts.TryGetValue(id, out var a))
///  		{
/// 			a.Text = text;
/// 			return;
/// 		}
/// 
/// 		AffectedTexts[id] = new LuceneEntity {Address = id, Site = site, Text = text};
/// 	}
/// 
/// 	public void DeindexText(EntityTextField field, EntityId entity, EntityId site)
/// 	{
/// 		var id = new EntityFieldAddress(entity, field);
/// 
///  		if(AffectedTexts.TryGetValue(id, out var a))
///  		{
/// 			a.Deleted = true;
/// 			return;
/// 		}
/// 
/// 		AffectedTexts[id] = new LuceneEntity {Address = id, Site = site, Deleted = true};
/// 	}
 
/// 	public LuceneEntity FindText(EntityField id)
/// 	{
/// 		if(AffectedTexts.TryGetValue(id, out var a))
/// 			return a;
///
///		var docs = Mcv.LuceneSearcher.Search(new TermQuery(new Term("id", new Lucene.Net.Util.BytesRef((id as IBinarySerializable).Raw))), 1);
///
///		if(docs.TotalHits > 0)
///			return new LuceneEntity() {Entity = id, Text = Mcv.LuceneSearcher.Doc(docs.ScoreDocs[0].Doc).Get("t")};
///		else
///			return null;
/// 	}
///
///	public LuceneEntity AffectedText(EntityField id)
///	{
///		if(AffectedTexts.TryGetValue(id, out var a))
///			return a;
///			
///		/// This can be skipped since there is no cause when we need to read current text value
///		/// 
///		///var docs = Mcv.LuceneSearcher.Search(new TermQuery(new Term("id", new Lucene.Net.Util.BytesRef((id as IBinarySerializable).Raw))), 1);
///		///
///		///if(docs.TotalHits > 0)
///		///	return AffectedTexts[id] = new LuceneEntity() {Entity = id, Text = Mcv.LuceneSearcher.Doc(docs.ScoreDocs[0].Doc).Get("t")};
///		///else
///			return AffectedTexts[id] = new LuceneEntity() {Entity = id};
///	}


/// 	public void IndexText(string text, EntityTextField field, EntityId entity)
/// 	{
/// 		foreach(var w in text.Split(' '))
/// 		{
/// 			for(int n = 1; n <= 5; n++)
/// 			{
/// 				for(int j = 0; j <= w.Length - n; j++)
/// 				{
/// 					var id = Ngram.GetId(n, w, j);
/// 				
/// 					var t = AffectNgram(id);
/// 
/// 					var e = t.References.FirstOrDefault(e => e.Field == field && e.Entity == entity);
/// 			
/// 					if(e == null)
/// 					{
/// 						t.References = [..t.References, new WordReference {Entity = entity, Field = field}];
/// 					}
/// 
/// 					if(n > 1)
/// 					{
/// 						var p = AffectNgram(Ngram.GetId(n - 1, w, j));
/// 	
/// 						if(!p.Ngrams.Contains(id))	/// add parent
/// 							p.Ngrams = [..p.Ngrams, id];
/// 
/// 						if(j == w.Length - n)		/// all prev parent are prefixes, the last one is postfix [.. parent]
/// 						{
/// 							p = AffectNgram(Ngram.GetId(n - 1, w, j + 1));
/// 					
/// 							if(!p.Ngrams.Contains(id))
/// 								p.Ngrams = [..p.Ngrams, id];
/// 						}
/// 					}
/// 				}
/// 			}
/// 		}
/// 	}
/// 
/// 	public void DeindexText(string text, EntityTextField field, EntityId entity)
/// 	{
/// 		foreach(var i in text.Split(' '))
/// 		{
/// 			var w = i;
/// 			
/// 			if(w.Length < 3)
/// 			{
/// 				w = w.PadLeft(3, '\0');
/// 			}
/// 
/// 			if(w.Length == 3)
/// 			{
/// 				var id = Ngram.GetId(3, w, 0);
/// 
/// 				var t = AffectNgram(id);
/// 				var e = t.References.FirstOrDefault(e => e.Field == field && e.Entity == entity);
/// 		
/// 				if(e == null)
/// 				{
/// 					t.References = t.References.Remove(e);
/// 				}
/// 			}
/// 
/// 			for(int j=0; j <= w.Length - 4; j++)
/// 			{
/// 				var id = Ngram.GetId(4, w, j);
/// 				
/// 				if(w.Length == 4)
/// 				{
/// 					var t = AffectNgram(id);
/// 					var e = t.References.FirstOrDefault(e => e.Field == field && e.Entity == entity);
/// 		
/// 					if(e == null)
/// 					{
/// 						t.References = t.References.Remove(e);
/// 					}
/// 				} 
/// 			}
/// 
/// 			for(int j=0; j <= w.Length - 5; j++)
/// 			{
/// 				var id = Ngram.GetId(5, w, j);
/// 				
/// 				if(w.Length >= 5)
/// 				{
/// 					var t = AffectNgram(id);
/// 					var e = t.References.FirstOrDefault(e => e.Field == field && e.Entity == entity);
/// 		
/// 					if(e == null)
/// 					{
/// 						t.References = t.References.Remove(e);
/// 					}
/// 				} 
/// 			}
/// 		}
///	
}
