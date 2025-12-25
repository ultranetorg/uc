namespace Uccs.Fair;

public class FairExecution : Execution
{
	public new Fair						Net => base.Net as Fair;
	public new FairMcv					Mcv => base.Mcv as FairMcv;
	public new FairRound				Round => base.Round as FairRound;

	public AuthorExecution				Authors;
	public ProductExecution				Products;
	public SiteExecution				Sites;
	public CategoryExecution			Categories;
	public PublicationExecution			Publications;
	public ReviewExecution				Reviews;
	public ProposalExecution			Proposals;
	public ProposalCommentExecution		ProposalComments;
	public FileExecution				Files;
	public WordExecution				Words;
	public PublicationTitleExecution	PublicationTitles;
	public SiteTitleExecution			SiteTitles;

	public bool							SkipPowCheck;

	public FairExecution(FairMcv mcv, FairRound round, Transaction transaction) : base(mcv, round, transaction)
	{
		Authors				= new(this);
		Products			= new(this);
		Sites				= new(this);
		Categories			= new(this);
		Publications		= new(this);
		Reviews				= new(this);
		Proposals			= new(this);
		ProposalComments	= new(this);
		Files				= new(this);
		Words				= new(this);
		PublicationTitles	= new(this);
		SiteTitles			= new(this);
	}

	public FairExecution CreateChild()
	{
		var e = new FairExecution(Mcv, Round, Transaction);

		e.Parent = this;

		e.EnergySpenders	= [];
		e.SpacetimeSpenders	= [];
		e.ECEnergyCost		= ECEnergyCost;

		e.Authors			= new(this){Parent = Authors};
		e.Products			= new(this){Parent = Products};
		e.Sites				= new(this){Parent = Sites};
		e.Categories		= new(this){Parent = Categories};
		e.Publications		= new(this){Parent = Publications};
		e.Reviews			= new(this){Parent = Reviews};
		e.Proposals			= new(this){Parent = Proposals};
		e.ProposalComments	= new(this){Parent = ProposalComments};
		e.Files				= new(this){Parent = Files};
		e.Words				= new(this){Parent = Words};
		e.PublicationTitles	= new(this){Parent = PublicationTitles};
		e.SiteTitles		= new(this){Parent = SiteTitles};

		return e;
	}


	public void Allocate(Author author, Publisher publisher, int space, out string error)
	{
		error = null;

		if(publisher.SpacetimeLimit != Publisher.Unlimit)
		{
			publisher.SpacetimeLimit -= space;

			if(publisher.SpacetimeLimit < 0)
			{
				error = Operation.LimitReached;
				return;
			}
		}
		
		Allocate(author, author, space);
	}

	public void Absorb(FairExecution execution)
	{
		foreach(var i in execution.AffectedUsers)
			AffectedUsers[i.Key] = i.Value;

		foreach(var i in execution.AffectedMetas)
			AffectedMetas[i.Key] = i.Value;

		Authors				.Absorb(execution.Authors);
		Products			.Absorb(execution.Products);
		Sites				.Absorb(execution.Sites);
		Categories			.Absorb(execution.Categories);
		Publications		.Absorb(execution.Publications);
		Reviews				.Absorb(execution.Reviews);
		Proposals			.Absorb(execution.Proposals);
		ProposalComments	.Absorb(execution.ProposalComments);
		Files				.Absorb(execution.Files);
		Words				.Absorb(execution.Words);
		PublicationTitles	.Absorb(execution.PublicationTitles);
		SiteTitles			.Absorb(execution.SiteTitles);

		foreach(var i in execution.EnergySpenders) /// This  may add a duplicated clone but we expect this is ok
			EnergySpenders.Add(i);

		foreach(var i in execution.SpacetimeSpenders) /// This  may add a duplicated clone but we expect this is ok
			SpacetimeSpenders.Add(i);
	}

	public override ITableExecution FindExecution(byte table)
	{
		if(table == Mcv.Authors.Id)				return Authors;
		if(table == Mcv.Products.Id)			return Products;
		if(table == Mcv.Sites.Id)				return Sites;
		if(table == Mcv.Categories.Id)			return Categories;
		if(table == Mcv.Publications.Id)		return Publications;
		if(table == Mcv.Reviews.Id)				return Reviews;
		if(table == Mcv.Proposals.Id)			return Proposals;
		if(table == Mcv.ProposalComments.Id)	return ProposalComments;
		if(table == Mcv.Files.Id)				return Files;
		if(table == Mcv.Words.Id)				return Words;

		return base.FindExecution(table);
	}

	public override ITableEntry Affect(byte table, EntityId id)
	{
		if(table == Mcv.Authors.Id)				return Authors.Find(id as AutoId)				!= null	? Authors.Affect(id as AutoId) : null;
		if(table == Mcv.Sites.Id)				return Sites.Find(id as AutoId)					!= null	? Sites.Affect(id as AutoId) : null;
		//if(table == Mcv.Products.Id)			return Products.Find(id as AutoId)				!= null	? Products.Affect(id as AutoId) : null;
		//if(table == Mcv.Categories.Id)		return Categories.Find(id as AutoId)			!= null	? Categories.Affect(id as AutoId) : null;
		//if(table == Mcv.Publications.Id)		return Publications.Find(id as AutoId)			!= null	? Publications.Affect(id as AutoId) : null;
		//if(table == Mcv.Reviews.Id)			return FindReview(id as AutoId)					!= null	? Reviews.Affect(id as AutoId) : null;
		//if(table == Mcv.Disputes.Id)			return FindDispute(id as AutoId)				!= null	? AffectDispute(id as AutoId) : null;
		//if(table == Mcv.Words.Id)				return FindWord(id as RawId)					!= null	? AffectWord(id as RawId) : null;
		//if(table == Mcv.PublicationTitles.Id)	return Mcv.PublicationTitles.Find(id as HnswId)	!= null	? AffectPublicationTitle(id as RawId) : null;

		return base.Affect(table, id);
	}

	public override System.Collections.IDictionary AffectedByTable(TableBase table)
	{
		if(table == Mcv.Authors)			return Authors.Affected;
		if(table == Mcv.Products)			return Products.Affected;
		if(table == Mcv.Sites)				return Sites.Affected;
		if(table == Mcv.Categories)			return Categories.Affected;
		if(table == Mcv.Publications)		return Publications.Affected;
		if(table == Mcv.Reviews)			return Reviews.Affected;
		if(table == Mcv.Proposals)			return Proposals.Affected;
		if(table == Mcv.ProposalComments)	return ProposalComments.Affected;
		if(table == Mcv.Files)				return Files.Affected;
		if(table == Mcv.Words)				return Words.Affected;
		if(table == Mcv.PublicationTitles)	return PublicationTitles.Affected;
		if(table == Mcv.SiteTitles)			return SiteTitles.Affected;

		return base.AffectedByTable(table);
	}

	public new FairUser FindUser(AutoId id)
	{
		return base.FindUser(id) as FairUser;
	}

	public new FairUser AffectUser(AutoId id)
	{
		return base.AffectUser(id) as FairUser;
	}

	public override FairUser CreateUser(string name, AccountAddress address)
	{
		var a = base.CreateUser(name, address) as FairUser;

		a.Reviews = [];
		a.ModeratedSites = [];
		a.Authors = [];
		a.Registrations = [];
		a.FavoriteSites = [];
		a.Nickname = "";

		return a;
	}

	public void DeleteAccount(FairUser account)
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

//	public File AllocateFile(AutoId creator, AutoId current, ISpacetimeHolder holder, ISpaceConsumer consumer, byte[] data)
//	{
//		if(current != null)
//		{
//			var p = Files.Affect(current); /// previous
//			p.Deleted = true;
//			
//			Free(holder, consumer, p.Data.Length);
//		}
//
//		File f = null;
//
//		if(data != null)
//		{
//			f = Files.Create(creator);
//			f.Data = data;
//
//			Allocate(holder, consumer, f.Data.Length);
//		}
//
//		return f;
//	 }

//	public void Allocate(Execution execution, int space)
//	{
//		if(space == 0)
//			return;
//
//		SpacetimeConsumer.Space += space;
//
//		var n = SpacetimeConsumer.Expiration - execution.Time.Days;
//	
//		SpacetimePayer.Spacetime -= ToBD(space, (short)n);
//
//		for(int i = 0; i < n; i++)
//			execution.Spacetimes[i] += space;
//
//		SpacetimeSpenders.Add(SpacetimePayer);
//	}

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
