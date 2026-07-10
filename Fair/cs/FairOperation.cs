using System.Text.RegularExpressions;

namespace Uccs.Fair;

public enum FairOperationClass : uint
{
	User							= 001,
		FairUser					= 001_001,
			UserAvatarChange		= 001_001_001,
			FavoriteStoreChange		= 001_001_002,

	Author							= 101, 
		AuthorCreation				= 101_000_001, 
		AuthorRenewal				= 101_000_002,
		AuthorModerationReward		= 101_000_003,
		AuthorOwnerAddition			= 101_000_004,
		AuthorOwnerRemoval			= 101_000_005,
		AuthorNameChange			= 101_000_006,
		AuthorAvatarChange			= 100_000_007,
		AuthorInfoUpdation			= 100_000_008,
		PublisherLimitsUpdation		= 100_000_009,
		AuthorVerification			= 100_000_010,
	
	Product							= 102, 
		ProductCreation				= 102_000_001, 
		ProductUpdation				= 102_000_002, 
		ProductDeletion				= 102_000_999,
	
	Store							= 103,
		StoreCreation				= 103_000_001, 
		StoreRenewal				= 103_000_002,
		StoreApprovalPolicyChange	= 103_000_003,
		StoreModeratorAddition		= 103_000_004,
		StoreModeratorRemoval		= 103_000_005,
		StoreAuthorsRemoval			= 103_000_006,
		StoreInfoUpdation			= 103_000_007,
		StoreAvatarChange			= 103_000_008,
		StoreNameChange				= 103_000_009,
		UserRegistration			= 103_000_010,
		UserUnregistration			= 103_000_011,
		StoreDeletion				= 103_000_999,

		Proposal						= 103_001,
			ProposalCreation			= 103_001_001,
			ProposalVoting				= 103_001_002,
			PerpetualVoting				= 103_001_003,

		Category						= 103_002,
			CategoryCreation			= 103_002_001,
			CategoryMovement			= 103_002_002,
			CategoryAvatarChange		= 103_002_003,
			CategoryTypeChange			= 103_002_004,
			CategoryDeletion			= 103_002_999,

		Publication						= 103_003,
			PublicationCreation			= 103_003_001,
			PublicationPublish			= 103_003_002,
			PublicationUnpublish		= 103_003_003,
			PublicationUpdation			= 103_003_004,
			PublicationAuthorPermittance= 103_003_005,
			PublicationPrioritization	= 103_003_006,
			PublicationDeletion			= 103_003_999,

		Review							= 103_004,
			ReviewCreation				= 103_004_001,
			ReviewStatusChange			= 103_004_002,
			ReviewEdit					= 103_004_003,
			ReviewDeletion				= 103_004_999,
		
		ProposalComment					= 103_006,
			ProposalCommentCreation		= 103_006_001,
			ProposalCommentEdit			= 103_006_002,
	
	File								= 104, 
		FileCreation					= 104_000_001, 
		FileDeletion					= 104_000_999,
} 

public abstract class StoreOperation : FairOperation
{
	public Store	Store;
}

public abstract class VotableOperation : StoreOperation
{
	public Role					As;
	public AutoId				By;

	public abstract bool		ValidateProposal(FairExecution execution, out string error);
 	public abstract bool		Overlaps(VotableOperation other);
}

public abstract class FairOperation : Operation
{
	//public const string			InvalidProposal = "Invalid proposal";
	public const string			CategoryNotSet = "Category not set";
	public const string			NotEmpty = "Not empty";
	public const string			NotNewUser = "Not a new user";
	public const string			Ended = "Ended";
	public const string			InvalidOwnerAddress = "Invalid Owner Type";
	public const string			DoesNotBelogToStore = "Does not belong to a store";
	public const string			NotEmptyReferencies = "Not Empty References";
	public const string			TypeAlreadyDefined = "Type already defined";
	public const string			NotPublished = "Not published";
	public const string			NotSupported = "Not supported";
	public const string			NotApproved = "Not approved";

	public new FairUser			User { get => base.User as FairUser; set => base.User = value; }

	public abstract void		Execute(FairExecution execution);


	public override void Execute(Execution execution)
	{
		Execute(execution as FairExecution);
	}

	public bool AccountExists(FairExecution execution, AutoId id, out FairUser account, out string error)
	{
		var r = base.AccountExists(execution, id, out var a, out error);
		
		account = a as FairUser;

		return r;
	}

	public bool CanAccessAccount(FairExecution execution, AutoId id, out FairUser account, out string error)
	{
		var r = base.CanAccessAccount(execution, id, out var a, out error);
		
		account = a as FairUser;

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

		if(author.IsExpired(execution.Time))
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

		if(!author.Owners.Contains(User.Id))
		{
			error = Denied;
			return false;
		}

		return true;
	}

	public bool IsPublisher(FairExecution execution, Store store, AutoId authorid, out Publisher publisher, out string error)
	{
		if(!CanAccessAuthor(execution, authorid, out _, out error))
		{	
			publisher = null;
			return false;
		}

		publisher = store.Publishers.FirstOrDefault(i => i.Author == authorid);

 		if(publisher == null)
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

	public bool CanAccessPublication(FairExecution execution, AutoId id, out Publication publication, out Author author, out Product product, out string error)
	{
		product = null;
		author	= null;

		if(!PublicationExists(execution, id, out publication, out error))
			return false; 

		product = execution.Products.Find(publication.Product);

		if(!CanAccessAuthor(execution, product.Author, out author, out error))
			return false; 

		return true; 
	}

	public bool StoreExists(FairExecution execution, AutoId id, out Store store, out string error)
	{
		store = execution.Stores.Find(id);
		
		if(store == null)
		{
			error = NotFound;
			return false; 
		}

		error = null;
		return true; 
	}

	public bool IsModerator(FairExecution execution, AutoId storeid, out Store store, out string error)
	{
 		if(!StoreExists(execution, storeid, out store, out error))
 			return false; 

		var m = store.Moderators.FirstOrDefault(i => i.User == User.Id);

		if(m == null || m.BannedTill > execution.Time)
		{
			error = Denied;
			return false; 
		}

		return true; 
	}

	public bool IsModerator(FairExecution execution, Store store, AutoId accountid, out Moderator moderator, out string error)
	{
		moderator = store.Moderators.FirstOrDefault(i => i.User == accountid);

		if(moderator == null)
		{
			error = Denied;
			return false; 
		}

		error = null;
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

 	public bool CanModerateCategory(FairExecution execution, AutoId id, out Category category, out Store store, out string error)
 	{
		store = null;

 		if(!CategoryExists(execution, id, out category, out error))
 			return false; 
 
 		if(!IsModerator(execution, category.Store, out store, out error))
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

 	public bool CamModeratePublication(FairExecution execution, AutoId id, User signer, out Publication publication, out Store store, out string error)
 	{
		store = null;

 		if(!PublicationExists(execution, id, out publication, out error))
 			return false; 
 
 		if(!IsModerator(execution, publication.Store, out store, out error))
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

	public bool IsReviewOwner(FairExecution execution, AutoId id, User signer, out Review review, out string error)
	{
		if(!ReviewExists(execution, id, out review, out error))
			return false;

		if(review.Creator != User.Id)
			return false;

 		//if(!PublicationExists(execution, review.Publication, out var p, out error))
 		//	return false; 
 
		error = null;
		return true;
	}

	public bool CanModerateReview(FairExecution execution, AutoId id, User signer, out Review review, out Store store, out string error)
 	{
		store = null;

 		if(!ReviewExists(execution, id, out review, out error))
 			return false; 

 		if(!CamModeratePublication(execution, review.Publication, User, out var p, out store, out error))
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

 	public bool IsReferendumCommentOwner(FairExecution execution, Store store, AutoId commentid, out Publisher citizen, out Proposal proposal, out ProposalComment comment, out string error)
 	{
		citizen = null;
		proposal = null;
		comment = execution.ProposalComments.Find(commentid);
		
		if(comment == null || comment.Deleted)
		{
			error = NotFound;
			return false; 
		}
 
		proposal = execution.Proposals.Find(comment.Proposal);

		if(!IsPublisher(execution, store, comment.Creator, out citizen, out error))
			return false;

		if(comment.Creator != citizen.Author)
		{
			error = Denied;
			return false; 
		}

		error = null;
 		return true;
 	}

 	public bool CanModerateDisputeComment(FairExecution execution, AutoId commentid, out Store store, out Proposal proposal, out ProposalComment comment, out string error)
 	{
		store = null;
		proposal = null;
		comment = execution.ProposalComments.Find(commentid);
		
		if(comment == null || comment.Deleted)
		{
			error = NotFound;
			return false; 
		}
 
		proposal = execution.Proposals.Find(comment.Proposal);

		if(!IsModerator(execution, proposal.Store, out store, out error))
			return false;

		error = null;
 		return true;
 	}

	public bool CanAccessFile(FairExecution execution, AutoId id, EntityAddress owner, out File file, out string error)
	{
		if(!FileExists(execution, id, out file, out error))
			return false;

		if((FairTable)file.Owner.Table == FairTable.Author && !CanAccessAuthor(execution, file.Owner.Id, out _, out error))
			return false;
		else if((FairTable)file.Owner.Table == FairTable.Store && !IsModerator(execution, file.Owner.Id, out _, out error))
			return false;

		return true;
	}

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

	public bool IsImage(File file, out string error)
	{
 		if(file.Mime != FairMime.ImageJpg && file.Mime != FairMime.ImagePng)
		{
			error = NotSupported;
			return false;
		}

		error = null;
		return true;
	}
}