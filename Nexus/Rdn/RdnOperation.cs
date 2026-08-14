namespace Uccs.Rdn;

public enum RdnOperationClass : uint
{
	RdnGenesis					= OperationClass.Genesis, 
	RdnCandidacyDeclaration		= OperationClass.CandidacyDeclaration, 

	User							= 100,
		UserRenaming				= 100_000_001, 

	Domain							= 101,
		DomainRegistration			= 101_000_001, 
		DomainMigration				= 101_000_002, 
		DomainRenewal				= 101_000_003,
		DomainTransfer				= 101_000_004,
		DomainPolicyUpdation		= 101_000_005,
		//DomainBid					= 101_000_003, 

	Resource						= 102,
		ResourceCreation			= 102_000_001, 
		ResourceUpdation			= 102_000_002, 
		ResourceDeletion			= 102_000_003, 

		ResourceLink				= 102_001, 
			ResourceLinkCreation	= 102_001_001, 
			ResourceLinkDeletion	= 102_001_002,

	Analysis						= 103,
		AnalysisResultUpdation		= 103_000_001
}

public abstract class RdnOperation : Operation
{
	public const string		CantChangeSealedResource = "Cant change sealed resource";
	public const string		CircularDependency = "Circular dependency";
	public const string		NoData = "No data";
	public const string		NotRoot = "Not root domain";
	public const string		NotDependable = "Not dependable";
	public const string		OtherTldHasPriority = "Other tld has priority";
	public const string		Locked = "Locked";

	public abstract void Execute(RdnExecution execution);

	public override void Execute(Execution execution)
	{
		Execute(execution as RdnExecution);
	}

	public bool RequireDomain(RdnExecution execution, AutoId id, out Domain domain)
	{
		domain = execution.Domains.Find(id);

		if(domain == null || domain.Deleted)
		{
			Error = NotFound;
			return false;
		}

		if(domain.IsExpired(execution.Round.ConsensusTime))
		{
			Error = Expired;
			return false;
		}

		return true;
	}

	public bool RequireDomain(RdnExecution execution, string name, out Domain domain)
	{
		domain = execution.Domains.Find(name);

		if(domain == null || domain.Deleted)
		{
			Error = NotFound;
			return false;
		}

		if(domain.IsExpired(execution.Round.ConsensusTime))
		{
			Error = Expired;
			return false;
		}

		return true;
	}

	public bool RequireDomainAccess(RdnExecution execution, string name, out Domain domain)
	{
		if(!RequireDomain(execution, name, out domain))
			return false;

		if(domain.Owner != User.Id)
		{
			Error = Denied;
			return false;
		}

		return true;
	}

	public bool RequireSignerDomain(RdnExecution execution, AutoId id, out Domain domain)
	{
		if(!RequireDomain(execution, id, out domain))
			return false;

		if(domain.Owner != User.Id)
		{
			Error = Denied;
			return false;
		}

		return true;
	}

	public bool RequireResource(RdnExecution execution, AutoId id, out Domain domain, out Resource resource)
	{
		resource = execution.Resources.Find(id);

		if(resource == null || resource.Deleted)
		{
			domain = null;
			Error = NotFound;
			return false;
		}

		if(!RequireDomain(execution, resource.Domain, out domain))
			return false; 

		return true;
	}

	public bool RequireSignerResource(RdnExecution execution, AutoId id, out Domain domain, out Resource resource)
	{
		if(!RequireResource(execution, id, out domain, out resource))
			return false; 

		if(!RequireSignerDomain(execution, resource.Domain, out _))
			return false; 

		return true; 
	}
}
