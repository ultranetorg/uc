namespace Uccs.Fair;

public enum FairOperationClass
{
	FairCandidacyDeclaration	= OperationClass.CandidacyDeclaration, 
	UtilityTransfer				= OperationClass.UtilityTransfer, 
	BandwidthAllocation			= OperationClass.BandwidthAllocation,

	Author							= 101, 
		AuthorCreation				= 101_000_001, 
		AuthorUpdation				= 101_000_002,
	
	Product							= 102, 
		ProductCreation				= 102_000_001, 
		ProductUpdation				= 102_000_002, 
		ProductDeletion				= 102_000_003,
	
	Site							= 103,
		SiteCreation				= 103_000_001, 
		SiteDeletion				= 103_000_002,
	
	Store							= 105,
		ModeratorAddition			= 105_000,

		Category					= 105_001,
			CategoryCreation		= 105_001_001,
			CategoryUpdation		= 105_001_002,
			CategoryDeletion		= 105_001_003,

		Publication					= 105_002,
			PublicationCreation		= 105_002_001,
			PublicationUpdation		= 105_002_002,
			PublicationDeletion		= 105_002_003,

		Review						= 105_003,
			ReviewCreation			= 105_003_001,
			ReviewUpdation			= 105_003_002,
			ReviewDeletion			= 105_003_003,
		
} 

public abstract class FairOperation : Operation
{
	public const string				CantChangeSealedProduct = "Cant change sealed resource";
	public const string				NotRoot = "Not root domain";
	public const string				AlreadyChild = "Already a child";

	public new FairAccountEntry		Signer => base.Signer as FairAccountEntry;

	public abstract void Execute(FairMcv mcv, FairRound round);

	public override void Execute(Mcv mcv, Round round)
	{
		Execute(mcv as FairMcv, round as FairRound);
	}

	public void PayForSpacetime(int length, Time time)
	{
		var fee = SpacetimeFee(length, time);
		
		Signer.BYBalance -= fee;
	}

	public static long SpacetimeFee(int length, Time time)
	{
		return Mcv.ApplyTimeFactor(time, length);
	}

	public void Allocate(Round round, Author author, int toallocate)
	{
		if(author.SpaceReserved < author.SpaceUsed + toallocate)
		{
			var f = SpacetimeFee(author.SpaceUsed + toallocate - author.SpaceReserved, author.Expiration - round.ConsensusTime);

			Signer.BYBalance -= f;

			author.SpaceReserved = 
			author.SpaceUsed = author.SpaceUsed + toallocate;
		}
		else
			author.SpaceUsed += toallocate;
	}

	public void Free(Author domain, int amount) /// WE DONT REFUND
	{
		//var f = SpacetimeFee(tofree, domain.Expiration - round.ConsensusTime);

		domain.SpaceUsed -= amount;
	
		//Signer.STBalance += f;
		//STReward -= f;
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

		if(RequireAuthor(round, product.AuthorId, out author) == false)
			return false; 

		return true; 
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

		if(!RequireAuthorAccess(round, product.AuthorId, out author))
			return false; 

		return true; 
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
