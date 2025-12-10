namespace Uccs.Rdn;

public enum RdnOperationClass : uint
{
	RdnGenesis					= OperationClass.Genesis, 
	RdnCandidacyDeclaration		= OperationClass.CandidacyDeclaration, 

	Domain							= 100,
		DomainRegistration			= 100_000_001, 
		DomainMigration				= 100_000_002, 
		DomainRenewal				= 100_000_003,
		DomainTransfer				= 100_000_004,
		DomainPolicyUpdation		= 100_000_005,
		//DomainBid					= 100_000_003, 

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
	public const string		CantChangeSealedResource = "Cant change sealed resource";
	public const string		NotRoot = "Not root domain";
	public const string		ReservedForOwner = "Unknown web domain";
	public const string		Sealed = "Sealed";
	public const string		NotSealed = "NotSealed";
	public const string		NoData = "NoData";

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

		if(domain.Owner != Signer.Id)
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

		if(domain.Owner != Signer.Id)
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
