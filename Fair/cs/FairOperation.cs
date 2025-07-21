
namespace Uccs.Fair;

public enum FairOperationClass : uint
{
	FairCandidacyDeclaration		= OperationClass.CandidacyDeclaration, 
	
	AccountNicknameChange			= 100_000_000,
	AccountAvatarChange				= 100_000_001,
	FavoriteSiteChange				= 100_000_002,

	Author							= 101, 
		AuthorCreation				= 101_000_001, 
		AuthorRenewal				= 101_000_002,
		AuthorModerationReward		= 101_000_003,
		AuthorOwnerAddition			= 101_000_004,
		AuthorOwnerRemoval			= 101_000_005,
		AuthorNicknameChange		= 101_000_006,
		AuthorAvatarChange			= 100_000_007,
		AuthorTextChange			= 100_000_008,
	
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
		SiteTextChange				= 103_000_006,
		SiteAvatarChange			= 103_000_007,
		SiteNicknameChange			= 103_000_008,
		UserRegistration			= 103_000_009,
		UserDeletion				= 103_000_010,
		SiteDeletion				= 103_000_999,

		Category						= 103_001,
			CategoryCreation			= 103_001_001,
			CategoryMovement			= 103_001_002,
			CategoryAvatarChange		= 103_001_003,
			CategoryTypeChange			= 103_001_004,
			CategoryDeletion			= 103_001_999,

		Publication						= 103_002,
			PublicationCreation			= 103_002_001,
			//PublicationApproval			= 103_002_002,
			PublicationRemoveFromChanged= 103_002_003,
			PublicationPublish			= 103_002_004,
			PublicationUpdation			= 103_002_005,
			PublicationDeletion			= 103_002_999,

		Review							= 103_003,
			ReviewCreation				= 103_003_001,
			ReviewStatusChange			= 103_003_002,
			ReviewEdit					= 103_003_003,
			ReviewEditModeration		= 103_003_004,
			ReviewDeletion				= 103_003_999,

		Proposal						= 103_004,
			ProposalCreation			= 103_004_001,
			ProposalVoting				= 103_004_002,
		
		ProposalComment					= 103_005,
			ProposalCommentCreation		= 103_005_001,
			ProposalCommentEdit			= 103_005_002,
} 

public abstract class VotableOperation : FairOperation
{
	public Site				Site;
	public Role				As;
	public AutoId			By;

	public abstract bool	ValidateProposal(FairExecution execution, out string error);
 	public abstract bool	Overlaps(VotableOperation other);
}

public abstract class FairOperation : Operation
{
	public const string			NotAllowedForSponsoredAccount = "Not allowed for free account";
	//public const string			InvalidProposal = "Invalid proposal";
	public const string			CategoryNotSet = "Category not set";
	public const string			NotEmpty = "Not empty";
	public const string			Ended = "Ended";
	public const string			InvalidOwnerAddress = "Invalid Owner Type";
	public const string			DoesNotBelogToSite = "Does not belong to site";

	public new FairAccount		Signer { get => base.Signer as FairAccount; set => base.Signer = value; }

	public abstract void Execute(FairExecution execution);

	public override void Execute(Execution execution)
	{
		Execute(execution as FairExecution);
	}

	public bool AccountExists(FairExecution execution, AutoId id, out FairAccount account, out string error)
	{
		var r = base.AccountExists(execution, id, out var a, out error);
		
		account = a as FairAccount;

		return r;
	}

	public bool CanAccessAccount(FairExecution execution, AutoId id, out FairAccount account, out string error)
	{
		var r = base.CanAccessAccount(execution, id, out var a, out error);
		
		account = a as FairAccount;

		return r;
	}

	public bool AuthorExists(FairExecution execution, AutoId id, out Author author, out string error)
	{
		author = execution.Authors.Find(id);

		if(author == null || author.Deleted)
		{
			error = NotFound;
			return false;
		}

		if(Author.IsExpired(author, execution.Time))
		{
			error = Expired;
			return false;
		}

		error = null;
		return true;
	}

	public bool ProductExists(FairExecution execution, AutoId id, out Author author, out Product product, out string error)
	{
		author = null;
		product = execution.Products.Find(id);
		
		if(product == null || product.Deleted)
		{
			error = NotFound;
			return false; 
		}

		if(AuthorExists(execution, product.Author, out author, out error) == false)
			return false; 

		return true; 
	}

	public bool CanAccessAuthor(FairExecution execution, AutoId id, out Author author, out string error)
	{
		if(!AuthorExists(execution, id, out author, out error))
			return false;

		if(!author.Owners.Contains(Signer.Id))
		{
			error = Denied;
			return false;
		}

		return true;
	}

	public bool IsPublisher(FairExecution execution, AutoId siteid, AutoId authorid, out Site site, out Author author, out string error)
	{
		site = null;

		if(!CanAccessAuthor(execution, authorid, out author, out error))
			return false;

		site = execution.Sites.Find(siteid);

 		if(!site.Authors.Contains(authorid))
 		{
 			error = Denied;
 			return false;
 		}

		return true;
	}

	public bool CanAccessProduct(FairExecution execution, AutoId id, out Author author, out Product product, out string error)
	{
		if(!ProductExists(execution, id, out  author, out product, out error))
			return false; 

		if(!CanAccessAuthor(execution, product.Author, out author, out error))
			return false; 

		return true; 
	}

	public bool SiteExists(FairExecution execution, AutoId id, out Site site, out string error)
	{
		site = execution.Sites.Find(id);
		
		if(site == null || site.Deleted)
		{
			error = NotFound;
			return false; 
		}

		error = null;
		return true; 
	}

	public bool IsModerator(FairExecution execution, AutoId siteid, out Site site, out string error)
	{
 		if(!SiteExists(execution, siteid, out site, out error))
 			return false; 

		if(!site.Moderators.Contains(Signer.Id))
		{
			error = Denied;
			return false; 
		}

		return true; 
	}

	public bool IsModerator(FairExecution execution, AutoId siteid, AutoId accountid, out Site site, out string error)
	{
 		if(!SiteExists(execution, siteid, out site, out error))
 			return false; 

		if(!site.Moderators.Contains(accountid))
		{
			error = Denied;
			return false; 
		}

		return true; 
	}

	public bool CategoryExists(FairExecution execution, AutoId id, out Category category, out string error)
	{
		category = execution.Categories.Find(id);
		
		if(category == null || category.Deleted)
		{
			error = NotFound;
			return false; 
		}

		error = null;
		return true;
	}

 	public bool CanModerateCategory(FairExecution execution, AutoId id, out Category category, out Site site, out string error)
 	{
		site = null;

 		if(!CategoryExists(execution, id, out category, out error))
 			return false; 
 
 		if(!IsModerator(execution, category.Site, out site, out error))
 			return false;
 
		error = null;
 		return true;
 	}

	public bool PublicationExists(FairExecution execution, AutoId id, out Publication publication, out string error)
	{
		publication = execution.Publications.Find(id);
		
		if(publication == null || publication.Deleted)
		{
			error = NotFound;
			return false; 
		}

		error = null;
		return true;
	}

 	public bool CamModeratePublication(FairExecution execution, AutoId id, Account signer, out Publication publication, out Site site, out string error)
 	{
		site = null;

 		if(!PublicationExists(execution, id, out publication, out error))
 			return false; 
 
 		if(!IsModerator(execution, publication.Site, out site, out error))
 			return false;
 
		error = null;
 		return true;
 	}

	public bool ReviewExists(FairExecution execution, AutoId id, out Review review, out string error)
	{
		review = execution.Reviews.Find(id);
		
		if(review == null || review.Deleted)
		{
			error = NotFound;
			return false; 
		}

		error = null;
		return true;
	}

 	public bool IsReviewOwner(FairExecution execution, AutoId id, Account signer, out Review review, out string error)
 	{
 		if(!ReviewExists(execution, id, out review, out error))
 			return false; 

		if(review.Creator == Signer.Id)
			return true;

 		//if(!PublicationExists(execution, review.Publication, out var p, out error))
 		//	return false; 
 
		error = null;
 		return true;
 	}

 	public bool CanModerateReview(FairExecution execution, AutoId id, Account signer, out Review review, out Site site, out string error)
 	{
		site = null;

 		if(!ReviewExists(execution, id, out review, out error))
 			return false; 

 		if(!CamModeratePublication(execution, review.Publication, Signer, out var p, out site, out error))
 			return false; 
 
		error = null;
 		return true;
 	}

	public bool ProposalExists(FairExecution execution, AutoId id, out Proposal proposal, out string error)
	{
		proposal = execution.Proposals.Find(id);
		
		if(proposal == null || proposal.Deleted)
		{
			error = NotFound;
			return false; 
		}

		error = null;
		return true;
	}

 	public bool IsReferendumCommentOwner(FairExecution execution, AutoId commentid, out Site site, out Author author, out Proposal proposal, out ProposalComment comment, out string error)
 	{
		site = null;
		author = null;
		proposal = null;
		comment = execution.ProposalComments.Find(commentid);
		
		if(comment == null || comment.Deleted)
		{
			error = NotFound;
			return false; 
		}
 
		proposal = execution.Proposals.Find(comment.Proposal);

		if(!IsPublisher(execution, proposal.Site, comment.Creator, out site, out author, out error))
			return false;

		if(comment.Creator != author.Id)
		{
			error = Denied;
			return false; 
		}

		error = null;
 		return true;
 	}

 	public bool CanModerateDisputeComment(FairExecution execution, AutoId commentid, out Site site, out Proposal proposal, out ProposalComment comment, out string error)
 	{
		site = null;
		proposal = null;
		comment = execution.ProposalComments.Find(commentid);
		
		if(comment == null || comment.Deleted)
		{
			error = NotFound;
			return false; 
		}
 
		proposal = execution.Proposals.Find(comment.Proposal);

		if(!IsModerator(execution, proposal.Site, out site, out error))
			return false;

		error = null;
 		return true;
 	}

	//protected void PayEnergyBySite(FairExecution execution, AutoId site)
	//{
	//	var s = execution.Sites.Affect(site);
	//		
	//	EnergyFeePayer = s;
	//	EnergySpenders = [s];
	//}
	//
	//protected void PayEnergyByAuthor(FairExecution execution, AutoId author)
	//{
	//	var a = execution.Authors.Affect(author);
	//		
	//	EnergyFeePayer = a;
	//	EnergySpenders = [a];
	//}

	protected void RewardForModeration(FairExecution execution, Author author, Site site)
	{
		author.Energy -= author.ModerationReward;
		site.Energy += author.ModerationReward;
			
		execution.EnergySpenders.Add(author);
	}

	//protected void PayEnergyBySiteOrAuthor(FairExecution execution, Publication publication, Author author = null)
	//{
	//	if(publication.Flags.HasFlag(PublicationFlags.ApprovedByAuthor))
	//	{ 
	//		PayEnergyByAuthor(execution, author?.Id ?? execution.Products.Find(publication.Product).Author);
	//	}
	//	else
	//	{	
	//		PayEnergyBySite(execution, publication.Site);
	//	}
	//}

	public bool FileExists(FairExecution execution, AutoId id, out File file, out string error)
	{
		file = execution.Files.Find(id);
		
		if(file == null || file.Deleted)
		{
			error = NotFound;
			return false; 
		}

		error = null;
		return true;
	}
}
