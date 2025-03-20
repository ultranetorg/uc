
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
			PublicationStatusChange		= 103_002_002,
			PublicationProductChange	= 103_002_003,
			PublicationUpdateModeration	= 103_002_004,
			PublicationDeletion			= 103_002_999,

		Review						= 103_003,
			ReviewCreation			= 103_003_001,
			ReviewStatusChange		= 103_003_002,
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
	public const string				Ended = "Ended";

	public new FairAccountEntry		Signer => base.Signer as FairAccountEntry;

	public abstract void Execute(FairExecution execution, bool voted);

	public override void Execute(Execution execution)
	{
		Execute(execution as FairExecution, false);
	}

	public bool RequireAccount(FairExecution round, EntityId id, out FairAccountEntry account)
	{
		var r = base.RequireAccount(round, id, out var a);
		
		account = a as FairAccountEntry;

		return r;
	}

	public bool RequireAuthor(FairExecution round, EntityId id, out AuthorEntry author)
	{
		author = round.FindAuthor(id);

		if(author == null || author.Deleted)
		{
			Error = NotFound;
			return false;
		}

		if(Author.IsExpired(author, round.Time))
		{
			Error = Expired;
			return false;
		}

		return true;
	}

	public bool RequireProduct(FairExecution round, EntityId id, out AuthorEntry author, out ProductEntry product)
	{
		author = null;
		product = round.FindProduct(id);
		
		if(product == null || product.Deleted)
		{
			Error = NotFound;
			return false; 
		}

		if(RequireAuthor(round, product.Author, out author) == false)
			return false; 

		return true; 
	}

	public bool CanAccessAuthor(FairExecution round, EntityId id)
	{
		var r = RequireAuthorAccess(round, id, out var _);
		Error = null;
		return r;
	}

	public bool RequireAuthorAccess(FairExecution round, EntityId id, out AuthorEntry author)
	{
		author = round.FindAuthor(id);

		if(author == null || author.Deleted)
		{
			Error = NotFound;
			return false;
		}

		if(Author.IsExpired(author, round.Time))
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

	public bool RequireProductAccess(FairExecution round, EntityId id, out AuthorEntry author, out ProductEntry product)
	{
		if(!RequireProduct(round, id, out  author, out product))
			return false; 

		if(!RequireAuthorAccess(round, product.Author, out author))
			return false; 

		return true; 
	}

	public bool CanAccessSite(FairExecution round, EntityId id)
	{
		var r = RequireSiteModeratorAccess(round, id, out var _);
		Error = null;
		return r;
	}

	public bool RequireSite(FairExecution round, EntityId id, out SiteEntry site)
	{
		site = round.FindSite(id);
		
		if(site == null || site.Deleted)
		{
			Error = NotFound;
			return false; 
		}

		return true; 
	}

	public bool RequireSiteModeratorAccess(FairExecution round, EntityId id, out SiteEntry site)
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

	public bool RequireModeratorAccess(SiteEntry site)
	{
		if(!site.Moderators.Contains(Signer.Id))
		{
			Error = Denied;
			return false; 
		}

		return true; 
	}

	public bool RequireCategory(FairExecution round, EntityId id, out CategoryEntry category)
	{
		category = round.FindCategory(id);
		
		if(category == null || category.Deleted)
		{
			Error = NotFound;
			return false; 
		}

		return true;
	}

 	public bool RequireCategoryAccess(FairExecution round, EntityId id, out CategoryEntry category)
 	{
 		if(!RequireCategory(round, id, out category))
 			return false; 
 
 		if(!RequireSiteModeratorAccess(round, category.Site, out var s))
 			return false;
 
 		return true;
 	}

	public bool RequirePublication(FairExecution round, EntityId id, out PublicationEntry publication)
	{
		publication = round.FindPublication(id);
		
		if(publication == null || publication.Deleted)
		{
			Error = NotFound;
			return false; 
		}

		return true;
	}

 	public bool RequirePublicationModeratorAccess(FairExecution round, EntityId id, AccountEntry signer, out PublicationEntry publication, out SiteEntry site)
 	{
		site = null;

 		if(!RequirePublication(round, id, out publication))
 			return false; 
 
 		if(!RequireSiteModeratorAccess(round, round.FindCategory(publication.Category).Site, out site))
 			return false;
 
 		return true;
 	}

	public bool RequireReview(FairExecution round, EntityId id, out ReviewEntry review)
	{
		review = round.FindReview(id);
		
		if(review == null || review.Deleted)
		{
			Error = NotFound;
			return false; 
		}

		return true;
	}

 	public bool RequireReviewOwnerAccess(FairExecution round, EntityId id, AccountEntry signer, out ReviewEntry review)
 	{
 		if(!RequireReview(round, id, out review))
 			return false; 

		if(review.Creator == Signer.Id)
			return true;

 		if(!RequirePublication(round, review.Publication, out var p))
 			return false; 
 
 		return true;
 	}

 	public bool RequireReviewModertorAccess(FairExecution round, EntityId id, AccountEntry signer, out ReviewEntry review, out SiteEntry site)
 	{
		site = null;

 		if(!RequireReview(round, id, out review))
 			return false; 

 		if(!RequirePublicationModeratorAccess(round, review.Publication, Signer, out var p, out site))
 			return false; 
 
 		return true;
 	}

	public bool RequireDispute(FairExecution round, EntityId id, out DisputeEntry review)
	{
		review = round.FindDispute(id);
		
		if(review == null || review.Deleted)
		{
			Error = NotFound;
			return false; 
		}

		return true;
	}
}

public abstract class VotableOperation : FairOperation
{
	public abstract bool ValidProposal(FairExecution execution);
 	public abstract bool Overlaps(VotableOperation other);
	//public abstract void Execute(FairExecution mcv, bool validate);

	protected void PayForModeration(FairExecution round, Publication publication, AuthorEntry author)
	{
		var s = round.AffectSite(round.FindCategory(publication.Category).Site);

		author.Energy -= author.ModerationReward;
		Signer.Energy += author.ModerationReward;
			
		EnergyFeePayer = s;
		EnergySpenders.Add(s);
		EnergySpenders.Add(author);
	}

}