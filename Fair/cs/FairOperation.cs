
namespace Uccs.Fair;

public enum FairOperationClass : uint
{
	FairCandidacyDeclaration		= OperationClass.CandidacyDeclaration, 

	Author							= 100, 
		AuthorCreation				= 100_000_001, 
		AuthorRenewal				= 100_000_002,
		AuthorModerationReward		= 100_000_003,
		AuthorOwnerAddition			= 100_000_004,
		AuthorOwnerRemoval			= 100_000_005,
	
	Product							= 101, 
		ProductCreation				= 101_000_001, 
		ProductUpdation				= 101_000_002, 
		ProductDeletion				= 101_000_999,
	
	Site							= 102,
		SiteCreation				= 102_000_001, 
		SiteRewal					= 102_000_002,
		SitePolicyChange			= 102_000_003,
		SiteAuthorsChange			= 102_000_004,
		SiteModeratorsChange		= 102_000_005,
		SiteDeletion				= 102_000_999,
	
	Store							= 103,
		ModeratorAddition			= 103_000_001,

		Category					= 103_001,
			CategoryCreation		= 103_001_001,
			CategoryMovement		= 103_001_002,
			CategoryDeletion		= 103_001_999,

		Publication						= 103_002,
			PublicationCreation			= 103_002_001,
			PublicationStatusUpdation	= 103_002_002,
			PublicationProductUpdation	= 103_002_003,
			PublicationChangeModeration	= 103_002_004,
			PublicationDeletion			= 103_002_999,

		Review						= 103_003,
			ReviewCreation			= 103_003_001,
			ReviewStatusUpdation	= 103_003_002,
			ReviewTextUpdation		= 103_003_003,
			ReviewTextModeration	= 103_003_004,
			ReviewDeletion			= 103_003_999,

		Dispute						= 103_004,
			DisputeCreation			= 103_004_001,
			DisputeVoting			= 103_004_002,
		
} 

public abstract class FairOperation : Operation
{
	public const string				NotAllowedForFreeAccount = "Not allowed for free account";
	public const string				InvalidProposal = "Invalid proposal";

	public new FairAccountEntry		Signer => base.Signer as FairAccountEntry;

	public abstract void Execute(FairMcv mcv, FairRound round);

	public override void Execute(Mcv mcv, Round round)
	{
		Execute(mcv as FairMcv, round as FairRound);
	}

	public virtual bool ValidProposal(FairMcv mcv, FairRound round, Site site)
 	{
		return true;
 	}

 	public virtual bool Overlaps(FairOperation other)
 	{
		return false;
 	}

	public virtual void Execute(FairMcv mcv, FairRound round, SiteEntry site)
	{
 	}

	public bool RequireAccount(Round round, EntityId id, out FairAccountEntry account)
	{
		var r = base.RequireAccount(round, id, out var a);
		
		account = a as FairAccountEntry;

		return r;
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

		if(!author.Owners.Contains(Signer.Id))
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

	public bool RequireSite(FairRound round, EntityId id, out SiteEntry site)
	{
		site = round.Mcv.Sites.Find(id, round.Id);
		
		if(site == null || site.Deleted)
		{
			Error = NotFound;
			return false; 
		}

		return true; 
	}

	public bool RequireSiteAccess(FairRound round, EntityId id, out SiteEntry site)
	{
 		if(!RequireSite(round, id, out site))
 			return false; 

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

	public bool RequireDispute(FairRound round, EntityId id, out DisputeEntry review)
	{
		review = round.Mcv.Disputes.Find(id, round.Id);
		
		if(review == null || review.Deleted)
		{
			Error = NotFound;
			return false; 
		}

		return true;
	}
}
