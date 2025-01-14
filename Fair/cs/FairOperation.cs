namespace Uccs.Fair;

public enum FairOperationClass
{
	None = 0, 
	FairCandidacyDeclaration= OperationClass.CandidacyDeclaration, 
	//Immission				= OperationClass.Immission, 
	UtilityTransfer			= OperationClass.UtilityTransfer, 
	BandwidthAllocation		= OperationClass.BandwidthAllocation,

	ChildNetInitialization,

	AuthorCreation, AuthorUpdation,
	ProductCreation, ProductUpdation, ProductDeletion,
	SiteCreation, SiteDeletion,
	PageCreation, PageUpdation, PageDeletion,
}

public abstract class FairOperation : Operation
{
	public const string				CantChangeSealedProduct = "Cant change sealed resource";
	public const string				NotRoot = "Not root domain";

	public new FairAccountEntry		Signer => base.Signer as FairAccountEntry;

	public abstract void Execute(FairMcv mcv, FairRound round);

	public override void Execute(Mcv mcv, Round round)
	{
		Execute(mcv as FairMcv, round as FairRound);
	}

	public void PayForSpacetime(int length, Time time)
	{
		var fee = SpacetimeFee(length, time);
		
		Signer.BYBalance	 -= fee;
		Transaction.BYReward += fee;
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

			Signer.BYBalance	 -= f;
			Transaction.BYReward += f;

			author.SpaceReserved = 
			author.SpaceUsed = (short)(author.SpaceUsed + toallocate);
		}
		else
			author.SpaceUsed += (short)toallocate;
	}

	public void Free(Author domain, int amount) /// WE DONT REFUND
	{
		//var f = SpacetimeFee(tofree, domain.Expiration - round.ConsensusTime);

		domain.SpaceUsed -= (short)amount;
	
		//Signer.STBalance += f;
		//STReward -= f;
	}

	public bool RequireSignerAuthor(FairRound round, EntityId id, out AuthorEntry domain)
	{
		domain = round.Mcv.Authors.Find(id, round.Id);

		if(domain == null)
		{
			Error = NotFound;
			return false;
		}

		if(Author.IsExpired(domain, round.ConsensusTime))
		{
			Error = Expired;
			return false;
		}

		if(domain.Owner != Signer.Id)
		{
			Error = Denied;
			return false;
		}

		return true;
	}

	public bool RequireAuthor(FairRound round, EntityId id, out AuthorEntry domain)
	{
		domain = round.Mcv.Authors.Find(id, round.Id);

		if(domain == null)
		{
			Error = NotFound;
			return false;
		}

		if(Author.IsExpired(domain, round.ConsensusTime))
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
		
		if(product == null)
		{
			Error = NotFound;
			return false; 
		}

		if(RequireAuthor(round, product.AuthorId, out author) == false)
		{
			Error = NotFound;
			return false; 
		}

		return true; 
	}

	public bool RequireSignerProduct(FairRound round, EntityId id, out AuthorEntry author, out ProductEntry product)
	{
		author = null;
		product = round.Mcv.Products.Find(id, round.Id);
		
		if(product == null)
		{
			Error = NotFound;
			return false; 
		}

		if(RequireSignerAuthor(round, product.AuthorId, out author) == false)
		{
			Error = NotFound;
			return false; 
		}


		return true; 
	}

	public bool RequireSiteAccess(FairRound round, EntityId id, out SiteEntry site)
	{
		site = round.Mcv.Sites.Find(id, round.Id);
		
		if(site == null)
		{
			Error = NotFound;
			return false; 
		}

		if(!site.Owners.Contains(Signer.Id))
		{
			Error = Denied;
			return false; 
		}

		return true; 
	}

	public bool RequirePageAccess(FairRound round, EntityId id, out SiteEntry site, out PageEntry card)
	{
		site = null;
		card = round.Mcv.Pages.Find(id, round.Id);
		
		if(card == null)
		{
			Error = NotFound;
			return false; 
		}

		return RequireSiteAccess(round, card.Site, out site); 
	}
}
