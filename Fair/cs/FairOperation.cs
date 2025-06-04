
namespace Uccs.Fair;

public enum FairOperationClass : uint
{
	FairCandidacyDeclaration		= OperationClass.CandidacyDeclaration, 
	
	NicknameChange					= 100_000_000,
	FavoriteSiteChange				= 100_000_001,

	Author							= 101, 
		AuthorCreation				= 101_000_001, 
		AuthorRenewal				= 101_000_002,
		AuthorModerationReward		= 101_000_003,
		AuthorOwnerAddition			= 101_000_004,
		AuthorOwnerRemoval			= 101_000_005,
	
	Product							= 102, 
		ProductCreation				= 102_000_001, 
		ProductUpdation				= 102_000_002, 
		ProductDeletion				= 102_000_999,
	
	Site							= 103,
		SiteCreation				= 103_000_001, 
		SiteRenewal					= 103_000_002,
		SitePolicyChange			= 103_000_003,
		SiteAuthorsChange			= 103_000_004,
		SiteModeratorsChange		= 103_000_005,
		SiteDescriptionChange		= 103_000_006,
		SiteDeletion				= 103_000_999,
	
	Store							= 104,
		ModeratorAddition				= 104_000_001,

		Category						= 104_001,
			CategoryCreation			= 104_001_001,
			CategoryMovement			= 104_001_002,
			CategoryDeletion			= 104_001_999,

		Publication						= 104_002,
			PublicationCreation			= 104_002_001,
			PublicationApproval			= 104_002_002,
			PublicationProductChange	= 104_002_003,
			PublicationCategoryChange	= 104_002_004,
			PublicationUpdation			= 104_002_005,
			PublicationDeletion			= 104_002_999,

		Review							= 104_003,
			ReviewCreation				= 104_003_001,
			ReviewStatusChange			= 104_003_002,
			ReviewTextUpdation			= 104_003_003,
			ReviewTextModeration		= 104_003_004,
			ReviewDeletion				= 104_003_999,

		Dispute							= 104_004,
			DisputeCreation				= 104_004_001,
			DisputeVoting				= 104_004_002,
		
		DisputeComment					= 104_005,
			DisputeCommentCreation		= 104_005_001,
			DisputeCommentTextChange	= 104_005_002,
} 

public abstract class FairOperation : Operation
{
	public const string				NotAllowedForFreeAccount = "Not allowed for free account";
	public const string				InvalidProposal = "Invalid proposal";
	public const string				CategoryNotSet = "Category not set";
	public const string				NotEmpty = "Not empty";
	public const string				Ended = "Ended";

	public new FairAccount		Signer => base.Signer as FairAccount;

	public abstract void Execute(FairExecution execution, bool voted);

	public override void Execute(Execution execution)
	{
		Execute(execution as FairExecution, false);
	}

	public bool RequireAccount(FairExecution round, AutoId id, out FairAccount account)
	{
		var r = base.RequireAccount(round, id, out var a);
		
		account = a as FairAccount;

		return r;
	}

	public bool RequireAuthor(FairExecution round, AutoId id, out Author author)
	{
		author = round.Authors.Find(id);

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

	public bool RequireProduct(FairExecution round, AutoId id, out Author author, out Product product)
	{
		author = null;
		product = round.Products.Find(id);
		
		if(product == null || product.Deleted)
		{
			Error = NotFound;
			return false; 
		}

		if(RequireAuthor(round, product.Author, out author) == false)
			return false; 

		return true; 
	}

	public bool CanAccessAuthor(FairExecution execution, AutoId id)
	{
		var r = RequireAuthorAccess(execution, id, out var _);
		Error = null;
		return r;
	}

	public bool RequireAuthorAccess(FairExecution round, AutoId id, out Author author)
	{
		author = round.Authors.Find(id);

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

	public bool RequireAuthorMembership(FairExecution execution, AutoId siteid, AutoId authorid, out Site site, out Author author)
	{
		site = null;

		if(!RequireAuthorAccess(execution, authorid, out author))
			return false;

		site = execution.Sites.Find(siteid);

 		if(!site.Authors.Contains(authorid))
 		{
 			Error = Denied;
 			return false;
 		}

		return true;
	}

	public bool RequireProductAccess(FairExecution round, AutoId id, out Author author, out Product product)
	{
		if(!RequireProduct(round, id, out  author, out product))
			return false; 

		if(!RequireAuthorAccess(round, product.Author, out author))
			return false; 

		return true; 
	}

	public bool CanAccessSite(FairExecution round, AutoId id)
	{
		var r = RequireModeratorAccess(round, id, out var _);
		Error = null;
		return r;
	}

	public bool RequireSite(FairExecution round, AutoId id, out Site site)
	{
		site = round.Sites.Find(id);
		
		if(site == null || site.Deleted)
		{
			Error = NotFound;
			return false; 
		}

		return true; 
	}

	public bool RequireModeratorAccess(FairExecution round, AutoId id, out Site site)
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

	public bool RequireCategory(FairExecution round, AutoId id, out Category category)
	{
		category = round.Categories.Find(id);
		
		if(category == null || category.Deleted)
		{
			Error = NotFound;
			return false; 
		}

		return true;
	}

 	public bool RequireCategoryAccess(FairExecution round, AutoId id, out Category category)
 	{
 		if(!RequireCategory(round, id, out category))
 			return false; 
 
 		if(!RequireModeratorAccess(round, category.Site, out var s))
 			return false;
 
 		return true;
 	}

	public bool RequirePublication(FairExecution round, AutoId id, out Publication publication)
	{
		publication = round.Publications.Find(id);
		
		if(publication == null || publication.Deleted)
		{
			Error = NotFound;
			return false; 
		}

		return true;
	}

 	public bool RequirePublicationModeratorAccess(FairExecution round, AutoId id, Account signer, out Publication publication, out Site site)
 	{
		site = null;

 		if(!RequirePublication(round, id, out publication))
 			return false; 
 
 		if(!RequireModeratorAccess(round, publication.Site, out site))
 			return false;
 
 		return true;
 	}

	public bool RequireReview(FairExecution round, AutoId id, out Review review)
	{
		review = round.Reviews.Find(id);
		
		if(review == null || review.Deleted)
		{
			Error = NotFound;
			return false; 
		}

		return true;
	}

 	public bool RequireReviewOwnerAccess(FairExecution round, AutoId id, Account signer, out Review review)
 	{
 		if(!RequireReview(round, id, out review))
 			return false; 

		if(review.Creator == Signer.Id)
			return true;

 		if(!RequirePublication(round, review.Publication, out var p))
 			return false; 
 
 		return true;
 	}

 	public bool RequireReviewModertorAccess(FairExecution round, AutoId id, Account signer, out Review review, out Site site)
 	{
		site = null;

 		if(!RequireReview(round, id, out review))
 			return false; 

 		if(!RequirePublicationModeratorAccess(round, review.Publication, Signer, out var p, out site))
 			return false; 
 
 		return true;
 	}

	public bool RequireDispute(FairExecution round, AutoId id, out Dispute review)
	{
		review = round.Disputes.Find(id);
		
		if(review == null || review.Deleted)
		{
			Error = NotFound;
			return false; 
		}

		return true;
	}

 	public bool RequireReferendumCommentAuthorAccess(FairExecution execution, AutoId commentid, out Site site, out Author author, out Dispute dispute, out DisputeComment comment)
 	{
		site = null;
		author = null;
		dispute = null;
		comment = execution.DisputeComments.Find(commentid);
		
		if(comment == null || comment.Deleted)
		{
			Error = NotFound;
			return false; 
		}
 
		dispute = execution.Disputes.Find(comment.Dispute);

		if(!RequireAuthorMembership(execution, dispute.Site, comment.Creator, out site, out author))
			return false;

 		return true;
 	}

 	public bool RequireDisputeCommentModeratorAccess(FairExecution execution, AutoId commentid, out Site site, out Dispute dispute, out DisputeComment comment)
 	{
		site = null;
		dispute = null;
		comment = execution.DisputeComments.Find(commentid);
		
		if(comment == null || comment.Deleted)
		{
			Error = NotFound;
			return false; 
		}
 
		dispute = execution.Disputes.Find(comment.Dispute);

		if(!RequireModeratorAccess(execution, dispute.Site, out site))
			return false;

 		return true;
 	}

	protected void PayEnergyBySite(FairExecution execution, AutoId site)
	{
		var s = execution.Sites.Affect(site);
			
		EnergyFeePayer = s;
		EnergySpenders = [s];
	}

	protected void PayEnergyByAuthor(FairExecution execution, AutoId author)
	{
		var a = execution.Authors.Affect(author);
			
		EnergyFeePayer = a;
		EnergySpenders = [a];
	}

	protected void PayEnergyForModeration(FairExecution execution, Publication publication, Author author = null)
	{
		if(publication.Flags.HasFlag(PublicationFlags.ApprovedByAuthor))
		{ 
			var a = execution.Authors.Affect(author?.Id ?? execution.Products.Find(publication.Product).Author);
			var s = execution.Sites.Affect(publication.Site);

			a.Energy	-= a.ModerationReward;
			s.Energy	+= a.ModerationReward;
			
			EnergyFeePayer = a;
			EnergySpenders.Add(a);
		}
		else
		{	
			PayEnergyBySite(execution, publication.Site);
		}

	}

	protected void PayEnergyBySiteOrAuthor(FairExecution execution, Publication publication, Author author = null)
	{
		if(publication.Flags.HasFlag(PublicationFlags.ApprovedByAuthor))
		{ 
			PayEnergyByAuthor(execution, author?.Id ?? execution.Products.Find(publication.Product).Author);
		}
		else
		{	
			PayEnergyBySite(execution, publication.Site);
		}
	}
}

public abstract class VotableOperation : FairOperation
{
	public abstract bool ValidProposal(FairExecution execution);
 	public abstract bool Overlaps(VotableOperation other);
}