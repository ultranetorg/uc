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

	public void PayForForever(int size)
	{
		Signer.BDBalance -= ToBD(size, Mcv.Forever);
	}

	public void PayForSpacetime(Round round, int size, Time start, Time duration)
	{
		if(size == 0)
			return;

		Signer.BDBalance -= ToBD(size, duration);

		var n = start.Days + duration.Days - round.ConsensusTime.Days;

		if(n > round.Spacetimes.Length)
			round.Spacetimes = [..round.Spacetimes, ..new long[n - round.Spacetimes.Length]];

		for(int i = 0; i < duration.Days; i++)
			round.Spacetimes[start.Days - round.ConsensusTime.Days + i] += size;
	}

	public void PayForName(string address, int years)
	{
		var fee = NameFee(years, address);
		
		Signer.BDBalance -= fee;
	}

	public static long ToBD(int length, Time time)
	{
		return time.Days * length;
	}

	public static int NameFee(int years, string address)
	{
		var l = Domain.IsWeb(address) ? address.Length : (address.Length - Domain.NormalPrefix.ToString().Length);

		l = Math.Min(l, 10);

		return 10_000 * Time.FromYears(years).Days / (l * l * l * l);
	}

	public void Allocate(Round round, Domain domain, int size)
	{
		domain.Space += size;

		var t = Time.FromDays(domain.Expiration.Days - round.ConsensusTime.Days + 1); /// Pay for one more day
	
		Signer.BDBalance -= ToBD(size, t);

		if(t.Days > round.Spacetimes.Length)
			round.Spacetimes = [..round.Spacetimes, ..new long[t.Days - round.Spacetimes.Length]];

		for(int i = 0; i < t.Days; i++)
			round.Spacetimes[i] += size;
	}

	public void Free(Round round, Domain domain, int size)
	{
		domain.Space -= size;

		var d = domain.Expiration.Days - round.ConsensusTime.Days;
		
		if(d > 0)
		{
			var t = Time.FromDays(d);
	
			Signer.BDBalance += ToBD(size, t);
	
			for(int i = 1; i < t.Days; i++)
				round.Spacetimes[i] -= size;
		}
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
