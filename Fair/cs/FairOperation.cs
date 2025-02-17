namespace Uccs.Fair;

public enum FairOperationClass
{
	FairCandidacyDeclaration		= OperationClass.CandidacyDeclaration, 

	Author							= 100, 
		AuthorCreation				= 100_000_001, 
		AuthorUpdation				= 100_000_002,
	
	Product							= 101, 
		ProductCreation				= 101_000_001, 
		ProductUpdation				= 101_000_002, 
		ProductDeletion				= 101_000_003,
	
	Site							= 102,
		SiteCreation				= 102_000_001, 
		SiteUpdation				= 102_000_002,
		SiteDeletion				= 102_000_999,
	
	Store							= 103,
		ModeratorAddition			= 103_000_001,

		Category					= 104_001,
			CategoryCreation		= 104_001_001,
			CategoryUpdation		= 104_001_002,
			CategoryDeletion		= 104_001_003,

		Publication					= 105_002,
			PublicationCreation		= 105_002_001,
			PublicationUpdation		= 105_002_002,
			PublicationDeletion		= 105_002_003,

		Review						= 106_003,
			ReviewCreation			= 106_003_001,
			ReviewUpdation			= 106_003_002,
			ReviewDeletion			= 106_003_003,
		
} 

public abstract class FairOperation : Operation
{
	public const string				CantChangeSealed = "Cant change sealed resource";
	public const string				NotRoot = "Not root domain";
	public const string				AlreadyChild = "Already a child";

	public new FairAccountEntry		Signer => base.Signer as FairAccountEntry;

	public abstract void Execute(FairMcv mcv, FairRound round);

	public override void Execute(Mcv mcv, Round round)
	{
		Execute(mcv as FairMcv, round as FairRound);
	}

	public bool RequireAuthor(FairRound round, EntityId id, out AuthorEntry author)
	{
		author = round.Mcv.Authors.Find(id, round.Id);

		if(author == null || author.Deleted)
		{
			Error = NotFound;
			return false;
		}

		if(Author.IsExpired(author, round.ConsensusTime))
		{
			Error = Expired;
			return false;
		}

		return true;
	}

	public bool RequireProduct(FairRound round, EntityId id, out AuthorEntry author, out ProductEntry product)
	{
		author = null;
		product = round.Mcv.Products.Find(id, round.Id);
		
		if(product == null || product.Deleted)
		{
			Error = NotFound;
			return false; 
		}

		if(RequireAuthor(round, product.Author, out author) == false)
			return false; 

		return true; 
	}

	public bool CanAccessAuthor(FairRound round, EntityId id)
	{
		var r = RequireAuthorAccess(round, id, out var _);
		Error = null;
		return r;
	}

	public bool RequireAuthorAccess(FairRound round, EntityId id, out AuthorEntry author)
	{
		author = round.Mcv.Authors.Find(id, round.Id);

		if(author == null || author.Deleted)
		{
			Error = NotFound;
			return false;
		}

		if(Author.IsExpired(author, round.ConsensusTime))
		{
			Error = Expired;
			return false;
		}

		if(author.Owner != Signer.Id)
		{
			Error = Denied;
			return false;
		}

		return true;
	}

	public bool RequireProductAccess(FairRound round, EntityId id, out AuthorEntry author, out ProductEntry product)
	{
		if(!RequireProduct(round, id, out  author, out product))
			return false; 

		if(!RequireAuthorAccess(round, product.Author, out author))
			return false; 

		return true; 
	}

	public bool CanAccessSite(FairRound round, EntityId id)
	{
		var r = RequireSiteAccess(round, id, out var _);
		Error = null;
		return r;
	}

	public bool RequireSiteAccess(FairRound round, EntityId id, out SiteEntry site)
	{
		site = round.Mcv.Sites.Find(id, round.Id);
		
		if(site == null || site.Deleted)
		{
			Error = NotFound;
			return false; 
		}

		if(!site.Moderators.Contains(Signer.Id))
		{
			Error = Denied;
			return false; 
		}

		return true; 
	}

	public bool RequireCategory(FairRound round, EntityId id, out CategoryEntry category)
	{
		category = round.Mcv.Categories.Find(id, round.Id);
		
		if(category == null || category.Deleted)
		{
			Error = NotFound;
			return false; 
		}

		return true;
	}

 	public bool RequireCategoryAccess(FairRound round, EntityId id, out CategoryEntry category)
 	{
 		if(!RequireCategory(round, id, out category))
 			return false; 
 
 		if(!RequireSiteAccess(round, category.Site, out var s))
 			return false;
 
 		return true;
 	}

	public bool RequirePublication(FairRound round, EntityId id, out PublicationEntry publication)
	{
		publication = round.Mcv.Publications.Find(id, round.Id);
		
		if(publication == null || publication.Deleted)
		{
			Error = NotFound;
			return false; 
		}

		return true;
	}

 	public bool RequirePublicationAccess(FairRound round, EntityId id, AccountEntry signer, out PublicationEntry publication)
 	{
 		if(!RequirePublication(round, id, out publication))
 			return false; 
 
 		if(!RequireSiteAccess(round, round.Mcv.Categories.Find(publication.Category, round.Id).Site, out var s))
 			return false;
 
 		return true;
 	}

	public bool RequireReview(FairRound round, EntityId id, out ReviewEntry review)
	{
		review = round.Mcv.Reviews.Find(id, round.Id);
		
		if(review == null || review.Deleted)
		{
			Error = NotFound;
			return false; 
		}

		return true;
	}

 	public bool RequireReviewAccess(FairRound round, EntityId id, AccountEntry signer, out ReviewEntry review)
 	{
 		if(!RequireReview(round, id, out review))
 			return false; 

		if(review.Creator == Signer.Id)
			return true;

 		if(!RequirePublication(round, review.Publication, out var p))
 			return false; 
 
 		return true;
 	}
}
