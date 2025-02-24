
namespace Uccs.Fair;

public class FairRound : Round
{
	public new FairMcv									Mcv => base.Mcv as FairMcv;
	public Dictionary<EntityId, AuthorEntry>			AffectedAuthors = new();
	public Dictionary<EntityId, ProductEntry>			AffectedProducts = new();
	public Dictionary<EntityId, SiteEntry>				AffectedSites = new();
	public Dictionary<EntityId, CategoryEntry>			AffectedCategories = new();
	public Dictionary<EntityId, PublicationEntry>		AffectedPublications = new();
	public Dictionary<EntityId, ReviewEntry>			AffectedReviews = new();
	public Dictionary<EntityId, DisputeEntry>			AffectedDisputes = new();

	public FairAccountEntry								FindAccount(EntityId id) => Mcv.Accounts.Find(id, Id) as FairAccountEntry;
	public AuthorEntry									FindAuthor(EntityId id) => Mcv.Authors.Find(id, Id);
	public ProductEntry									FindProduct(EntityId id) => Mcv.Products.Find(id, Id);
	public SiteEntry									FindSite(EntityId id) => Mcv.Sites.Find(id, Id);
	public CategoryEntry								FindCategory(EntityId id) => Mcv.Categories.Find(id, Id);
	public PublicationEntry								FindPublication(EntityId id) => Mcv.Publications.Find(id, Id);
	public ReviewEntry									FindReview(EntityId id) => Mcv.Reviews.Find(id, Id);
	public DisputeEntry									FindDispute(EntityId id) => Mcv.Disputes.Find(id, Id);

	public FairRound(FairMcv rds) : base(rds)
	{
		NextEids = Mcv.Tables.Select(i => new Dictionary<int, int>()).ToArray();
	}

	public override long AccountAllocationFee(Account account)
	{
		return FairOperation.ToBD(Net.EntityLength, Uccs.Net.Mcv.Forever);
	}

	public override System.Collections.IDictionary AffectedByTable(TableBase table)
	{
		if(table == Mcv.Authors)		return AffectedAuthors;
		if(table == Mcv.Products)		return AffectedProducts;
		if(table == Mcv.Sites)			return AffectedSites;
		if(table == Mcv.Categories)		return AffectedCategories;
		if(table == Mcv.Publications)	return AffectedPublications;
		if(table == Mcv.Reviews)		return AffectedReviews;
		if(table == Mcv.Disputes)		return AffectedDisputes;

		return base.AffectedByTable(table);
	}

	public override ITableEntry Affect(byte table, EntityId id)
	{
		if(table == Mcv.Authors.Id)			return AffectAuthor(id);
		if(table == Mcv.Products.Id)		return AffectProduct(id);
		if(table == Mcv.Sites.Id)			return AffectSite(id);
		if(table == Mcv.Categories.Id)		return AffectCategory(id);
		if(table == Mcv.Publications.Id)	return AffectPublication(id);
		if(table == Mcv.Reviews.Id)			return AffectReview(id);
		if(table == Mcv.Disputes.Id)		return AffectDispute(id);

		return base.Affect(table, id);
	}

	public override AccountEntry AffectSigner(Transaction transaction)
	{
		if(transaction.Operations.All(i => i.NonExistingSignerAllowed))
		{
			if(AffectedAccounts.FirstOrDefault(i => i.Value.Address == transaction.Signer).Value is AccountEntry a)
				return a;
		
			a = Mcv.Accounts.Find(transaction.Signer, Id - 1)?.Clone();	

			if(a != null)
				TransferECIfNeeded(a);
			else
				a = CreateAccount(transaction.Signer);

			AffectedAccounts[a.Address] = a;

			return a;
		}

		return base.AffectSigner(transaction);
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
			
		var e = Mcv.Authors.Find(id, Id - 1);

		e = AffectedAuthors[id] = e.Clone();

		TransferECIfNeeded(e);

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
		
		a = Mcv.Products.Find(id, Id - 1);

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
			
		return AffectedSites[a.Id] = a;
	}

	public SiteEntry AffectSite(EntityId id)
	{
		if(AffectedSites.TryGetValue(id, out var a))
			return a;
			
		var e = Mcv.Sites.Find(id, Id - 1);

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
			
		var e = Mcv.Categories.Find(id, Id - 1);

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
			
		var e = Mcv.Publications.Find(id, Id - 1);

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
			
		var e = Mcv.Reviews.Find(id, Id - 1);

		return AffectedReviews[id] = e.Clone();
	}

	public DisputeEntry CreateDispute(AccountEntry signer)
	{
		var b = Mcv.Accounts.KeyToBid(signer.Address);
		
		int e = GetNextEid(Mcv.Disputes, b);

		var a = Mcv.Disputes.Create();
		a.Id = new EntityId(b, e);

		return AffectedDisputes[a.Id] = a;
	}

	public DisputeEntry AffectDispute(EntityId id)
	{
		if(AffectedDisputes.TryGetValue(id, out var a))
			return a;
			
		var e = Mcv.Disputes.Find(id, Id - 1);

		return AffectedDisputes[id] = e.Clone();
	}

// 	public bool IsAllowed(Publication page, TopicChange change, AccountEntry signer)
// 	{
// 		if(page == null)
// 		{
// 			return Mcv.Sites.Find(page.Site, Id)?.Owners.Contains(signer.Id) ?? false;
// 		}
// 
// 		if(page.Security != null)
// 		{
// 			if(page.Security.Permissions.TryGetValue(change, out var a))
// 			{
// 				return a.Any(i =>	{
// 										switch(i)
// 										{
// 											case Actor.Owner:
// 												return Mcv.Sites.Find(page.Site, Id)?.Owners.Contains(signer.Id) ?? false;
// 
// 											case Actor.Creator:
// 												return page.Creator == signer.Id;
// 
// 											case Actor.SiteUser :
// 											{
// 												throw new NotImplementedException();
// 												break;
// 											}
// 
// 											default:
// 												return false;
// 										}
// 									});
// 			}
// 			else
// 				return false;
// 		}
// 
// 		if(Parent != null)
// 		{
// 			var p = Mcv.Publications.Find(page.Parent, Id);
// 
// 			return IsAllowed(p, change, signer);
// 		}
// 
// 		return false;
// 	}
// 
// 	public bool NotPermitted(Publication page, TopicChange permission, AccountEntry signer)
// 	{
// 		return !IsAllowed(page, permission, signer);
// 	}

	public override void RestartExecution()
	{
// 		AffectedAuthors.Clear();
// 		AffectedProducts.Clear();
// 		AffectedSites.Clear();
// 		AffectedCards.Clear();
// 
// 		NextAuthorEids.Clear();
// 		NextProductEids.Clear();
// 		NextSiteEids.Clear();
// 		NextCardEids.Clear();
	}

	public override void FinishExecution()
	{
	}

	public override void Elect(Vote[] votes, int gq)
	{
	}

	public override void CopyConfirmed()
	{
	}

	public override void RegisterForeign(Operation o)
	{
	}

	public override void ConfirmForeign()
	{
	}

	public override void WriteBaseState(BinaryWriter writer)
	{
		base.WriteBaseState(writer);

		writer.Write(Candidates, i => i.WriteCandidate(writer));  
		writer.Write(Members, i => i.WriteMember(writer));  
	}

	public override void ReadBaseState(BinaryReader reader)
	{
		base.ReadBaseState(reader);

		Candidates	= reader.Read<Generator>(m => m.ReadCandidate(reader)).Cast<Generator>().ToList();
		Members		= reader.Read<Generator>(m => m.ReadMember(reader)).Cast<Generator>().ToList();
	}

}

