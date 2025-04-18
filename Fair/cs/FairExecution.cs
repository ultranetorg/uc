﻿using System.Text;

namespace Uccs.Fair;

public class FairExecution : Execution
{
	public new Fair				Net => base.Net as Fair;
	public new FairMcv			Mcv => base.Mcv as FairMcv;
	public new FairRound		Round => base.Round as FairRound;

	public Dictionary<EntityId, Author>				AffectedAuthors = new();
	public Dictionary<EntityId, Product>			AffectedProducts = new();
	public Dictionary<EntityId, Site>				AffectedSites = new();
	public Dictionary<EntityId, Category>			AffectedCategories = new();
	public Dictionary<EntityId, Publication>		AffectedPublications = new();
	public Dictionary<EntityId, Review>				AffectedReviews = new();
	public Dictionary<EntityId, Dispute>			AffectedDisputes = new();
	public Dictionary<RawId, Word>					AffectedWords = new();
	public Dictionary<RawId, SiteTerm>				AffectedPublicationTitles = new();

	public FairExecution(FairMcv mcv, FairRound round, Transaction transaction) : base(mcv, round, transaction)
	{
	}

	public override ITableEntry Affect(byte table, BaseId id)
	{
		if(table == Mcv.Authors.Id)				return FindAuthor(id as EntityId)		!= null	? AffectAuthor(id as EntityId) : null;
		if(table == Mcv.Products.Id)			return FindProduct(id as EntityId)		!= null	? AffectProduct(id as EntityId) : null;
		if(table == Mcv.Sites.Id)				return FindSite(id as EntityId)			!= null	? AffectSite(id as EntityId) : null;
		if(table == Mcv.Categories.Id)			return FindCategory(id as EntityId)		!= null	? AffectCategory(id as EntityId) : null;
		if(table == Mcv.Publications.Id)		return FindPublication(id as EntityId)	!= null	? AffectPublication(id as EntityId) : null;
		if(table == Mcv.Reviews.Id)				return FindReview(id as EntityId)		!= null	? AffectReview(id as EntityId) : null;
		if(table == Mcv.Disputes.Id)			return FindDispute(id as EntityId)		!= null	? AffectDispute(id as EntityId) : null;
		if(table == Mcv.Words.Id)				return FindWord(id as RawId)			!= null	? AffectWord(id as RawId) : null;
		if(table == Mcv.PublicationTitles.Id)	return FindPublicationTitle(id as RawId)!= null	? AffectPublicationTitle(id as RawId) : null;

		return base.Affect(table, id);
	}

	public new FairAccount FindAccount(EntityId id)
	{
		return base.FindAccount(id) as FairAccount;
	}

	public new FairAccount AffectAccount(EntityId id)
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

 	public Author FindAuthor(EntityId id)
 	{ 
 		if(AffectedAuthors.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.Authors.Find(id, Round.Id);
 	}

	public Author CreateAuthor(AccountAddress signer)
	{
		var b = Mcv.Accounts.KeyToBid(signer);
		
		int e = GetNextEid(Mcv.Authors, b);

		var a = Mcv.Authors.Create();
		a.Id = new EntityId(b, e);
		a.Products = [];
		a.Owners = [];
		a.Sites = [];
		a.Nickname = "";			

		return AffectedAuthors[a.Id] = a;
	}

	public Author AffectAuthor(EntityId id)
	{
		if(AffectedAuthors.TryGetValue(id, out var a))
			return a;
			
		var e = Mcv.Authors.Find(id, Round.Id);

		e = AffectedAuthors[id] = e.Clone();

		TransferEnergyIfNeeded(e);

		return e;
	}

	private void DeleteAuthor(Author author)
	{
	}
 
 	public Product FindProduct(EntityId id)
 	{
 		if(AffectedProducts.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.Products.Find(id, Round.Id);
 	}

	public Product CreateProduct(Author author)
	{
		int e = GetNextEid(Mcv.Products, author.Id.B);

  		var	p = new Product();

		p.Id = new EntityId(author.Id.B, e);
		p.Fields = []; 
		p.Publications = [];

  		return AffectedProducts[p.Id] = p;
	}

	public Product AffectProduct(EntityId id)
	{
		if(AffectedProducts.TryGetValue(id, out var a))
			return a;
		
		a = Mcv.Products.Find(id, Round.Id);

		return AffectedProducts[id] = a.Clone();
	}

 	public Site FindSite(EntityId id)
 	{
 		if(AffectedSites.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.Sites.Find(id, Round.Id);
 	}

	public Site CreateSite(Account signer)
	{
		var b = Mcv.Accounts.KeyToBid(signer.Address);
		
		int e = GetNextEid(Mcv.Sites, b);

		var a = Mcv.Sites.Create();
		
		a.Id = new EntityId(b, e);
		a.Categories = [];
		a.Moderators = [];
		a.Authors = [];
		a.Disputes = [];
		a.ChangePolicies = [];
		a.Nickname = "";
		a.Description = "";
		
		return AffectedSites[a.Id] = a;
	}

	public Site AffectSite(EntityId id)
	{
		if(AffectedSites.TryGetValue(id, out var a))
			return a;
			
		var e = Mcv.Sites.Find(id, Round.Id);

		return AffectedSites[id] = e.Clone();
	}
 
 	public Category FindCategory(EntityId id)
 	{
 		if(AffectedCategories.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.Categories.Find(id, Round.Id);
 	}

	public Category CreateCategory(Site site)
	{
		int e = GetNextEid(Mcv.Categories, site.Id.B);

		var a = Mcv.Categories.Create();
		a.Id = new EntityId(site.Id.B, e);
		a.Categories = [];
		a.Publications = [];

		return AffectedCategories[a.Id] = a;
	}
		
	public Category AffectCategory(EntityId id)
	{
		if(AffectedCategories.TryGetValue(id, out var a))
			return a;
			
		var e = Mcv.Categories.Find(id, Round.Id);

		return AffectedCategories[id] = e.Clone();
	}
 
 	public Publication FindPublication(EntityId id)
 	{
 		if(AffectedPublications.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.Publications.Find(id, Round.Id);
 	}

	public Publication CreatePublication(Site site)
	{
		int e = GetNextEid(Mcv.Publications, site.Id.B);

		var a = Mcv.Publications.Create();
		a.Id = new EntityId(site.Id.B, e);
		a.Fields = [];
		a.Changes = [];
		a.Reviews = [];
		a.ReviewChanges = [];
			
		return AffectedPublications[a.Id] = a;
	}

	public Publication AffectPublication(EntityId id)
	{
		if(AffectedPublications.TryGetValue(id, out var a))
			return a;
			
		var e = Mcv.Publications.Find(id, Round.Id);

		return AffectedPublications[id] = e.Clone();
	}
 
 	public Dispute FindDispute(EntityId id)
 	{
 		if(AffectedDisputes.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.Disputes.Find(id, Round.Id);
 	}

	public Review CreateReview(Publication publication)
	{
		int e = GetNextEid(Mcv.Reviews, publication.Id.B);

		var a = Mcv.Reviews.Create();
		a.Id = new EntityId(publication.Id.B, e);

		return AffectedReviews[a.Id] = a;
	}

	public Review AffectReview(EntityId id)
	{
		if(AffectedReviews.TryGetValue(id, out var a))
			return a;
			
		var e = Mcv.Reviews.Find(id, Round.Id);

		return AffectedReviews[id] = e.Clone();
	}

 	public Review FindReview(EntityId id)
 	{
 		if(AffectedReviews.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.Reviews.Find(id, Round.Id);
 	}

	public Dispute CreateDispute(Site site)
	{
		int e = GetNextEid(Mcv.Disputes, site.Id.B);

		var a = Mcv.Disputes.Create();
		a.Id = new EntityId(site.Id.B, e);
		a.Yes = [];
		a.No = [];
		a.Abs = [];

		return AffectedDisputes[a.Id] = a;
	}

	public Dispute AffectDispute(EntityId id)
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

	public void RegisterWord(string word, EntityTextField field, EntityId entity)
	{
		var id = Word.GetId(word);
		var w = FindWord(id)?.References.FirstOrDefault(e => e.Field == field && e.Entity == entity);;
		
		if(w == null)
		{
			var t = AffectWord(id);
	
			t.References = [..t.References, new EntityFieldAddress {Entity = entity, Field = field}];
		}
	}

	public void UnregisterWord(string word, EntityTextField field, EntityId entity)
	{
		var id = Word.GetId(word);
		var w = FindWord(id)?.References.FirstOrDefault(e => e.Field == field && e.Entity == entity);;
		
		if(w == null)
		{
			var t = AffectWord(id);
	
			t.References = t.References.Remove(w);
		}
	}

	 public SiteTerm FindPublicationTitle(RawId id)
 	{
 		if(AffectedPublicationTitles.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.PublicationTitles.Find(id, Round.Id);
 	}

	public SiteTerm AffectPublicationTitle(RawId id)
	{
		if(AffectedPublicationTitles.TryGetValue(id, out var a))
			return a;
			
		a = Mcv.PublicationTitles.Find(id, Round.Id);

		if(a == null)
		{
			a = Mcv.PublicationTitles.Create();
			a.Id = id;
			a.Children = [];
			a.References = [];
		
			return AffectedPublicationTitles[id] = a;
		} 
		else
		{
			return AffectedPublicationTitles[id] = a.Clone();
		}
	}

 	public void IndexPublicationTitle(EntityId site, string text, EntityId entity)
 	{
		foreach(var i in text.ToLowerInvariant().Split(' '))
		{
			var t = Mcv.PublicationTitles.Add(i, FindPublicationTitle, AffectPublicationTitle);
	
			t = AffectPublicationTitle(t.Id);
	
			if(!t.References.TryGetValue(site, out var eee))
				eee = [];
	
			if(!eee.Contains(entity))
			{	
				t.References = new (t.References);
				t.References[site] = [..eee, entity];
			}
		}
 	}

 	public void DeindexPublicationTitle(string text, Publication publication, EntityId site)
 	{
		foreach(var i in text.ToLowerInvariant().Split(' '))
		{
			var r = Mcv.PublicationTitles.Search(site, i, 0, 0, 1, null);
	
			var e = AffectPublicationTitle(r[0].Term.Id);
	
			e.References = new (e.References);
			e.References[site] = e.References[site].Remove(r[0].Entity);
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
