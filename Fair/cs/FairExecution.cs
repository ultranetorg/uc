﻿namespace Uccs.Fair;

public class FairExecution : Execution
{
	public new Fair				Net => base.Net as Fair;
	public new FairMcv			Mcv => base.Mcv as FairMcv;
	public new FairRound		Round => base.Round as FairRound;

	public Dictionary<EntityId, AuthorEntry>			AffectedAuthors = new();
	public Dictionary<EntityId, ProductEntry>			AffectedProducts = new();
	public Dictionary<EntityId, SiteEntry>				AffectedSites = new();
	public Dictionary<EntityId, CategoryEntry>			AffectedCategories = new();
	public Dictionary<EntityId, PublicationEntry>		AffectedPublications = new();
	public Dictionary<EntityId, ReviewEntry>			AffectedReviews = new();
	public Dictionary<EntityId, DisputeEntry>			AffectedDisputes = new();

	public FairExecution(FairMcv mcv, FairRound round, Transaction transaction) : base(mcv, round, transaction)
	{
	}

	public override ITableEntry Affect(byte table, EntityId id)
	{
		if(table == Mcv.Authors.Id)			return FindAuthor(id)		!= null	? AffectAuthor(id) : null;
		if(table == Mcv.Products.Id)		return FindProduct(id)		!= null	? AffectProduct(id) : null;
		if(table == Mcv.Sites.Id)			return FindSite(id)			!= null	? AffectSite(id) : null;
		if(table == Mcv.Categories.Id)		return FindCategory(id)		!= null	? AffectCategory(id) : null;
		if(table == Mcv.Publications.Id)	return FindPublication(id)	!= null	? AffectPublication(id) : null;
		if(table == Mcv.Reviews.Id)			return FindReview(id)		!= null	? AffectReview(id) : null;
		if(table == Mcv.Disputes.Id)		return FindDispute(id)		!= null	? AffectDispute(id) : null;

		return base.Affect(table, id);
	}

	public new FairAccountEntry FindAccount(EntityId id)
	{
		return base.FindAccount(id) as FairAccountEntry;
	}

 	public AuthorEntry FindAuthor(EntityId id)
 	{ 
 		if(AffectedAuthors.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.Authors.Find(id, Round.Id);
 	}
 
 	public ProductEntry FindProduct(EntityId id)
 	{
 		if(AffectedProducts.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.Products.Find(id, Round.Id);
 	}
 
 	public SiteEntry FindSite(EntityId id)
 	{
 		if(AffectedSites.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.Sites.Find(id, Round.Id);
 	}
 
 	public CategoryEntry FindCategory(EntityId id)
 	{
 		if(AffectedCategories.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.Categories.Find(id, Round.Id);
 	}
 
 	public PublicationEntry FindPublication(EntityId id)
 	{
 		if(AffectedPublications.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.Publications.Find(id, Round.Id);
 	}
 
 	public ReviewEntry FindReview(EntityId id)
 	{
 		if(AffectedReviews.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.Reviews.Find(id, Round.Id);
 	}
 
 	public DisputeEntry FindDispute(EntityId id)
 	{
 		if(AffectedDisputes.TryGetValue(id, out var a))
 			return a;
 
 		return Mcv.Disputes.Find(id, Round.Id);
 	}

	public override AccountEntry AffectSigner()
	{
		if(Transaction.Operations.All(i => i.NonExistingSignerAllowed))
		{
			if(AffectedAccounts.FirstOrDefault(i => i.Value.Address == Transaction.Signer).Value is AccountEntry a)
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

	public override FairAccountEntry CreateAccount(AccountAddress address)
	{
		var a = base.CreateAccount(address) as FairAccountEntry;

		a.Reviews = [];
		a.Sites = [];
		a.Authors = [];

		return a;
	}

	public void DeleteAccount(FairAccountEntry account)
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

	private void DeleteAuthor(AuthorEntry author)
	{
	}

	public new FairAccountEntry AffectAccount(EntityId id)
	{
		return base.AffectAccount(id) as FairAccountEntry;
	}

	public AuthorEntry CreateAuthor(AccountAddress signer)
	{
		var b = Mcv.Accounts.KeyToBid(signer);
		
		int e = GetNextEid(Mcv.Authors, b);

		var a = Mcv.Authors.Create();
		a.Id = new EntityId(b, e);
		a.Products = [];
		a.Owners = [];
		a.Sites = [];
			
		return AffectedAuthors[a.Id] = a;
	}

	public AuthorEntry AffectAuthor(EntityId id)
	{
		if(AffectedAuthors.TryGetValue(id, out var a))
			return a;
			
		var e = Mcv.Authors.Find(id, Round.Id);

		e = AffectedAuthors[id] = e.Clone();

		TransferEnergyIfNeeded(e);

		return e;
	}

	public ProductEntry CreateProduct(AuthorEntry author)
	{
		int e = GetNextEid(Mcv.Products, author.Id.B);

  		var	p = new ProductEntry {Id = new EntityId(author.Id.B, e), Fields = [], Publications = []};
    
  		return AffectedProducts[p.Id] = p;
	}

	public ProductEntry AffectProduct(EntityId id)
	{
		if(AffectedProducts.TryGetValue(id, out var a))
			return a;
		
		a = Mcv.Products.Find(id, Round.Id);

		return AffectedProducts[id] = a.Clone();
	}

	public SiteEntry CreateSite(AccountEntry signer)
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
			
		return AffectedSites[a.Id] = a;
	}

	public SiteEntry AffectSite(EntityId id)
	{
		if(AffectedSites.TryGetValue(id, out var a))
			return a;
			
		var e = Mcv.Sites.Find(id, Round.Id);

		return AffectedSites[id] = e.Clone();
	}

	public CategoryEntry CreateCategory(SiteEntry site)
	{
		int e = GetNextEid(Mcv.Categories, site.Id.B);

		var a = Mcv.Categories.Create();
		a.Id = new EntityId(site.Id.B, e);
		a.Categories = [];
		a.Publications = [];

		return AffectedCategories[a.Id] = a;
	}
		
	public CategoryEntry AffectCategory(EntityId id)
	{
		if(AffectedCategories.TryGetValue(id, out var a))
			return a;
			
		var e = Mcv.Categories.Find(id, Round.Id);

		return AffectedCategories[id] = e.Clone();
	}

	public PublicationEntry CreatePublication(SiteEntry site)
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

	public PublicationEntry AffectPublication(EntityId id)
	{
		if(AffectedPublications.TryGetValue(id, out var a))
			return a;
			
		var e = Mcv.Publications.Find(id, Round.Id);

		return AffectedPublications[id] = e.Clone();
	}

	public ReviewEntry CreateReview(PublicationEntry publication)
	{
		int e = GetNextEid(Mcv.Reviews, publication.Id.B);

		var a = Mcv.Reviews.Create();
		a.Id = new EntityId(publication.Id.B, e);

		return AffectedReviews[a.Id] = a;
	}

	public ReviewEntry AffectReview(EntityId id)
	{
		if(AffectedReviews.TryGetValue(id, out var a))
			return a;
			
		var e = Mcv.Reviews.Find(id, Round.Id);

		return AffectedReviews[id] = e.Clone();
	}

	public DisputeEntry CreateDispute(SiteEntry site)
	{
		int e = GetNextEid(Mcv.Disputes, site.Id.B);

		var a = Mcv.Disputes.Create();
		a.Id = new EntityId(site.Id.B, e);
		a.Yes = [];
		a.No = [];
		a.Abs = [];

		return AffectedDisputes[a.Id] = a;
	}

	public DisputeEntry AffectDispute(EntityId id)
	{
		if(AffectedDisputes.TryGetValue(id, out var a))
			return a;
			
		var e = Mcv.Disputes.Find(id, Round.Id);

		return AffectedDisputes[id] = e.Clone();
	}
}
