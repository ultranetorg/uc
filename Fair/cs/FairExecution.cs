namespace Uccs.Fair;

public class FairExecution : Execution
{
	public new Fair				Net => base.Net as Fair;
	public new FairMcv			Mcv => base.Mcv as FairMcv;
	public new FairRound		Round => base.Round as FairRound;

	public Dictionary<EntityId, Author>			AffectedAuthors = new();
	public Dictionary<EntityId, Product>		AffectedProducts = new();
	public Dictionary<EntityId, Site>			AffectedSites = new();
	public Dictionary<EntityId, Category>		AffectedCategories = new();
	public Dictionary<EntityId, Publication>	AffectedPublications = new();
	public Dictionary<EntityId, Review>			AffectedReviews = new();
	public Dictionary<EntityId, Dispute>		AffectedDisputes = new();
	public Dictionary<StringId, Text>			AffectedNicknames = new();

	public FairExecution(FairMcv mcv, FairRound round, Transaction transaction) : base(mcv, round, transaction)
	{
	}

	public override ITableEntry Affect(byte table, BaseId id)
	{
		if(table == Mcv.Authors.Id)			return FindAuthor(id as EntityId)		!= null	? AffectAuthor(id as EntityId) : null;
		if(table == Mcv.Products.Id)		return FindProduct(id as EntityId)		!= null	? AffectProduct(id as EntityId) : null;
		if(table == Mcv.Sites.Id)			return FindSite(id as EntityId)			!= null	? AffectSite(id as EntityId) : null;
		if(table == Mcv.Categories.Id)		return FindCategory(id as EntityId)		!= null	? AffectCategory(id as EntityId) : null;
		if(table == Mcv.Publications.Id)	return FindPublication(id as EntityId)	!= null	? AffectPublication(id as EntityId) : null;
		if(table == Mcv.Reviews.Id)			return FindReview(id as EntityId)		!= null	? AffectReview(id as EntityId) : null;
		if(table == Mcv.Disputes.Id)		return FindDispute(id as EntityId)		!= null	? AffectDispute(id as EntityId) : null;
		if(table == Mcv.Nicknames.Id)		return FindText(id as StringId)		!= null	? AffectText(id as StringId) : null;

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
		p.Nickname = "";

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
 
 	public Text FindText(StringId id)
 	{
 		if(AffectedNicknames.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.Nicknames.Find(id, Round.Id);
 	}

	public Text CreateText(string name)
	{
		var a = Mcv.Nicknames.Create();
		a.Id = new StringId(name);
		a.Entities = [];

		return AffectedNicknames[a.Id] = a;
	}

	public Text AffectText(StringId id)
	{
		if(AffectedNicknames.TryGetValue(id, out var a))
			return a;
			
		var e = Mcv.Nicknames.Find(id, Round.Id);

		return AffectedNicknames[id] = e.Clone();
	}
}
