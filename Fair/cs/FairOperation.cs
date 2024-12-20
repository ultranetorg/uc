namespace Uccs.Fair;

public enum FairOperationClass
{
	None = 0, 
	FairCandidacyDeclaration= OperationClass.CandidacyDeclaration, 
	//Immission				= OperationClass.Immission, 
	UtilityTransfer			= OperationClass.UtilityTransfer, 
	BandwidthAllocation		= OperationClass.BandwidthAllocation,

	ChildNetInitialization,

	AuthorRegistration, AuthorUpdation,
	ProductCreation, ProductUpdation, ProductDeletion
}

public abstract class FairOperation : Operation
{
	public const string		CantChangeSealedProduct = "Cant change sealed resource";
	public const string		NotRoot = "Not root domain";

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

	public void Allocate(Round round, Author domain, int toallocate)
	{
		if(domain.SpaceReserved < domain.SpaceUsed + toallocate)
		{
			var f = SpacetimeFee(domain.SpaceUsed + toallocate - domain.SpaceReserved, domain.Expiration - round.ConsensusTime);

			Signer.BYBalance	 -= f;
			Transaction.BYReward += f;

			domain.SpaceReserved = 
			domain.SpaceUsed = (short)(domain.SpaceUsed + toallocate);
		}
		else
			domain.SpaceUsed += (short)toallocate;
	}

	public void Free(Author domain, int tofree) /// WE DONT REFUND
	{
		//var f = SpacetimeFee(tofree, domain.Expiration - round.ConsensusTime);

		domain.SpaceUsed -= (short)tofree;
	
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
			Error = NotOwner;
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

	public bool RequireProduct(FairRound round, ProductId id, out AuthorEntry author, out ProductEntry product)
	{
		product = null;

		if(RequireAuthor(round, id.AuthorId, out author) == false)
		{
			Error = NotFound;
			return false; 
		}

		product = round.Mcv.Products.Find(id, round.Id);
		
		if(product == null)
		{
			Error = NotFound;
			return false; 
		}

		return true; 
	}

	public bool RequireSignerProduct(FairRound round, ProductId id, out AuthorEntry author, out ProductEntry product)
	{
		product = null;

		if(RequireSignerAuthor(round, id.AuthorId, out author) == false)
		{
			Error = NotFound;
			return false; 
		}

		product = round.Mcv.Products.Find(id, round.Id);
		
		if(product == null)
		{
			Error = NotFound;
			return false; 
		}

		return true; 
	}
}
