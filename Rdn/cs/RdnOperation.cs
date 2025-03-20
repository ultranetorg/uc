namespace Uccs.Rdn;

public enum RdnOperationClass : uint
{
	RdnCandidacyDeclaration		= OperationClass.CandidacyDeclaration, 

	Domain							= 100,
		DomainRegistration			= 100_000_001, 
		DomainMigration				= 100_000_002, 
		DomainBid					= 100_000_003, 
		DomainRenewal				= 100_000_004,
		DomainTransfer				= 100_000_005,
		DomainPolicyUpdation		= 100_000_006,

	Resource						= 101,
		ResourceCreation			= 101_000_001, 
		ResourceUpdation			= 101_000_002, 
		ResourceDeletion			= 101_000_003, 

		ResourceLink				= 101_001, 
			ResourceLinkCreation	= 101_001_001, 
			ResourceLinkDeletion	= 101_001_002,

	Analysis						= 102,
		AnalysisResultUpdation		= 102_000_001
}

public abstract class RdnOperation : Operation
{
	public const string					CantChangeSealedResource = "Cant change sealed resource";
	public const string					NotRoot = "Not root domain";
	public const string					Sealed = "Sealed";
	public const string					NotSealed = "NotSealed";
	public const string					NoData = "NoData";

	public abstract void Execute(RdnExecution execution);

	public override void Execute(Execution execution)
	{
		Execute(execution as RdnExecution);
	}

	public void PayForName(string address, int years)
	{
		var fee = NameFee(years, address);
		
		Signer.Spacetime -= fee;
		SpacetimeSpenders.Add(Signer);
	}

	public static int NameFee(int years, string address)
	{
		var l = Domain.IsWeb(address) ? address.Length : (address.Length - Domain.NormalPrefix.ToString().Length);

		l = Math.Min(l, 10);

		return 10_000 * Time.FromYears(years).Days / (l * l * l * l);
	}

	public void PayForForever(int size)
	{
		Signer.Spacetime -= ToBD(size, Mcv.Forever);
		SpacetimeSpenders.Add(Signer);
	}

	public bool RequireDomain(RdnExecution round, EntityId id, out DomainEntry domain)
	{
		domain = round.FindDomain(id);

		if(domain == null || domain.Deleted)
		{
			Error = NotFound;
			return false;
		}

		if(Domain.IsExpired(domain, round.Round.ConsensusTime))
		{
			Error = Expired;
			return false;
		}

		return true;
	}

	public bool RequireDomain(RdnExecution round, string name, out DomainEntry domain)
	{
		domain = round.FindDomain(name);

		if(domain == null || domain.Deleted)
		{
			Error = NotFound;
			return false;
		}

		if(Domain.IsExpired(domain, round.Round.ConsensusTime))
		{
			Error = Expired;
			return false;
		}

		return true;
	}

	public bool RequireDomainAccess(RdnExecution round, string name, out DomainEntry domain)
	{
		if(!RequireDomain(round, name, out domain))
			return false;

		if(domain.Owner != Signer.Id)
		{
			Error = Denied;
			return false;
		}

		return true;
	}

	public bool RequireSignerDomain(RdnExecution round, EntityId id, out DomainEntry domain)
	{
		if(!RequireDomain(round, id, out domain))
			return false;

		if(domain.Owner != Signer.Id)
		{
			Error = Denied;
			return false;
		}

		return true;
	}

	public bool RequireResource(RdnExecution round, EntityId id, out DomainEntry domain, out ResourceEntry resource)
	{
		resource = round.FindResource(id);

		if(resource == null || resource.Deleted)
		{
			domain = null;
			Error = NotFound;
			return false;
		}

		if(!RequireDomain(round, resource.Domain, out domain))
			return false; 

		return true;
	}

	public bool RequireSignerResource(RdnExecution round, EntityId id, out DomainEntry domain, out ResourceEntry resource)
	{
		if(!RequireResource(round, id, out domain, out resource))
			return false; 

		if(!RequireSignerDomain(round, resource.Domain, out _))
			return false; 

		return true; 
	}
}
