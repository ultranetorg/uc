namespace Uccs.Smp;

public class SmpRound : Round
{
	public new SmpMcv								Mcv => base.Mcv as SmpMcv;
	public Dictionary<EntityId, AuthorEntry>		AffectedAuthors = new();
	public Dictionary<EntityId, ProductEntry>		AffectedProducts = new();
	public Dictionary<EntityId, SiteEntry>			AffectedSites = new();
	public Dictionary<EntityId, CategoryEntry>		AffectedCategories = new();
	public Dictionary<EntityId, PublicationEntry>	AffectedPublications = new();
	public Dictionary<EntityId, ReviewEntry>		AffectedReviews = new();
	public Dictionary<int, int>						NextAuthorEids = new ();
	public Dictionary<int, int>						NextProductEids = new ();
	public Dictionary<int, int>						NextSiteEids = new ();
	public Dictionary<int, int>						NextCategoryEids = new ();
	public Dictionary<int, int>						NextPageEids = new ();
	public Dictionary<int, int>						NextReviewEids = new ();
	//public Dictionary<ushort, int>					NextAssortmentIds = new ();

	public SmpRound(SmpMcv rds) : base(rds)
	{
	}

	public override long AccountAllocationFee(Account account)
	{
		return SmpOperation.SpacetimeFee(Uccs.Net.Mcv.EntityLength, Uccs.Net.Mcv.Forever);
	}

	public override System.Collections.IDictionary AffectedByTable(TableBase table)
	{
		if(table == Mcv.Authors)		return AffectedAuthors;
		if(table == Mcv.Products)		return AffectedProducts;
		if(table == Mcv.Sites)			return AffectedSites;
		if(table == Mcv.Categories)		return AffectedCategories;
		if(table == Mcv.Publications)	return AffectedPublications;
		if(table == Mcv.Reviews)		return AffectedReviews;

		return base.AffectedByTable(table);
	}

	public override Dictionary<int, int> NextEidsByTable(TableBase table)
	{
		if(table == Mcv.Authors)		return NextAuthorEids;
		if(table == Mcv.Products)		return NextProductEids;
		if(table == Mcv.Sites)			return NextSiteEids;
		if(table == Mcv.Categories)		return NextCategoryEids;
		if(table == Mcv.Publications)	return NextPageEids;
		if(table == Mcv.Reviews)		return NextReviewEids;

		return base.NextEidsByTable(table);
	}

	public new SmpAccountEntry AffectAccount(AccountAddress address)
	{
		return base.AffectAccount(address) as SmpAccountEntry;
	}

	public new SmpAccountEntry AffectAccount(EntityId id)
	{
		return base.AffectAccount(id) as SmpAccountEntry;
	}

	public AuthorEntry CreateAuthor(AccountAddress signer)
	{
		var b = Mcv.Accounts.KeyToBid(signer);
		
		int e = GetNextEid(Mcv.Authors, b);

		var a = Mcv.Authors.Create();
		a.Id = new EntityId(b, e);
		a.Products = [];
			
		return AffectedAuthors[a.Id] = a;
	}

	public AuthorEntry AffectAuthor(EntityId id)
	{
		if(AffectedAuthors.TryGetValue(id, out var a))
			return a;
			
		var e = Mcv.Authors.Find(id, Id - 1);

		return AffectedAuthors[id] = e.Clone();
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
		a.Sections = [];
		a.Reviews = [];
			
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

