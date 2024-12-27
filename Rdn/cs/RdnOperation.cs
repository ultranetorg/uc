namespace Uccs.Rdn;

public enum RdnOperationClass
{
	None = 0, 
	RdnCandidacyDeclaration	= OperationClass.CandidacyDeclaration, 
	//Immission				= OperationClass.Immission, 
	UtilityTransfer			= OperationClass.UtilityTransfer, 
	BandwidthAllocation		= OperationClass.BandwidthAllocation,

	ChildNetInitialization,

	DomainRegistration, DomainMigration, DomainBid, DomainUpdation,
	ResourceCreation, ResourceUpdation, ResourceDeletion, ResourceLinkCreation, ResourceLinkDeletion,
	AnalysisResultUpdation
}

public abstract class RdnOperation : Operation
{
	public const string		CantChangeSealedResource = "Cant change sealed resource";
	public const string		NotRoot = "Not root domain";

	public abstract void Execute(RdnMcv mcv, RdnRound round);

	public override void Execute(Mcv mcv, Round round)
	{
		Execute(mcv as RdnMcv, round as RdnRound);
	}

	public void PayForSpacetime(int length, Time time)
	{
		var fee = SpacetimeFee(length, time);
		
		Signer.BYBalance	 -= fee;
		Transaction.BYReward += fee;
	}

	public void PayForName(string address, int years)
	{
		var fee = NameFee(years, address);
		
		Signer.BYBalance	 -= fee;
		Transaction.BYReward += fee;
	}

	public static long SpacetimeFee(int length, Time time)
	{
		return Mcv.ApplyTimeFactor(time, length);
	}

	public static long NameFee(int years, string address)
	{
		var l = Domain.IsWeb(address) ? address.Length : (address.Length - Domain.NormalPrefix.ToString().Length);

		l = Math.Min(l, 10);

		return Mcv.ApplyTimeFactor(Time.FromYears(years), 10_000) / (l * l * l * l);
	}

	public void Allocate(Round round, Domain domain, int toallocate)
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

	public void Free(Domain domain, int tofree) /// WE DONT REFUND
	{
		//var f = SpacetimeFee(tofree, domain.Expiration - round.ConsensusTime);

		domain.SpaceUsed -= (short)tofree;
	
		//Signer.STBalance += f;
		//STReward -= f;
	}

	public bool RequireSignerDomain(RdnRound round, string name, out DomainEntry domain)
	{
		domain = round.Mcv.Domains.Find(name, round.Id);

		if(domain == null)
		{
			Error = NotFound;
			return false;
		}

		if(Domain.IsExpired(domain, round.ConsensusTime))
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

	public bool RequireSignerDomain(RdnRound round, EntityId id, out DomainEntry domain)
	{
		domain = round.Mcv.Domains.Find(id, round.Id);

		if(domain == null)
		{
			Error = NotFound;
			return false;
		}

		if(Domain.IsExpired(domain, round.ConsensusTime))
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

	public bool RequireDomain(RdnRound round, EntityId id, out DomainEntry domain)
	{
		domain = round.Mcv.Domains.Find(id, round.Id);

		if(domain == null)
		{
			Error = NotFound;
			return false;
		}

		if(Domain.IsExpired(domain, round.ConsensusTime))
		{
			Error = Expired;
			return false;
		}

		return true;
	}

	public bool RequireResource(RdnRound round, ResourceId id, out DomainEntry domain, out ResourceEntry resource)
	{
		resource = null;

		if(RequireDomain(round, id, out domain) == false)
		{
			Error = NotFound;
			return false; 
		}

		resource = round.Mcv.Resources.Find(id, round.Id);
		
		if(resource == null)
		{
			Error = NotFound;
			return false; 
		}

		return true; 
	}

	public bool RequireSignerResource(RdnRound round, ResourceId id, out DomainEntry domain, out ResourceEntry resource)
	{
		resource = null;

		if(RequireSignerDomain(round, id, out domain) == false)
		{
			Error = NotFound;
			return false; 
		}

		resource = round.Mcv.Resources.Find(id, round.Id);
		
		if(resource == null)
		{
			Error = NotFound;
			return false; 
		}

		return true; 
	}
}
