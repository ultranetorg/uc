namespace Uccs.Rdn;

public enum RdnOperationClass : uint
{
	RdnGenesis					= OperationClass.Genesis, 
	RdnCandidacyDeclaration		= OperationClass.CandidacyDeclaration, 

	User							= 100,
		UserRenaming				= 100_000_001,

	DomainName						= 101,
		DomainNameAcquisition		= 101_000_001,
		DomainNameMigration			, 
		DomainNameRenewal			,
		DomainNameTransfer			,

	Domain							= 102,
		DomainCreation				= 102_000_001,
		DomainRenaming				, 
		DomainRenewal				,
		DomainTransfer				,
		DomainPolicyUpdation		,

	Resource						= 103,
		ResourceCreation			= 103_000_001,
		ResourceRenaming			,
		ResourceUpdation			, 
		ResourceDeletion			, 

		ResourceLink				= 103_001, 
			ResourceLinkCreation	= 103_001_001,
			ResourceLinkDeletion	,

	Analysis						= 104,
		AnalysisResultUpdation		= 104_000_001
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

	public new RdnUser		User { get => base.User as RdnUser; set => base.User = value; }

	public abstract void Execute(RdnExecution execution);

	public override void Execute(Execution execution)
	{
		Execute(execution as RdnExecution);
	}

	public bool RequireDomainName(RdnExecution execution, string name, out DomainName domainname)
	{
		domainname = execution.DomainNames.Find(name);

		if(domainname == null || domainname.Deleted)
		{
			Error = NotFound;
			return false;
		}

		if((domainname as IExpirable).IsExpired(execution.Time))
		{
			Error = Expired;
			return false;
		}

		return true;
	}
	
	public bool RequireDomainNameAccess(RdnExecution execution, string name, out DomainName domainname)
	{
		if(!RequireDomainName(execution, name, out domainname))
			return false;

		if(domainname.Owner != User.Id)
		{
			Error = Denied;
			return false;
		}

		return true;
	}

	public bool RequireDomain(RdnExecution execution, AutoId id, out Domain domain)
	{
		domain = execution.Domains.Find(id);

		if(domain == null || domain.Deleted)
		{
			Error = NotFound;
			return false;
		}

		if((domain as ISpaceConsumer).IsExpired(execution.Round.ConsensusTime))
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

		if((domain as ISpaceConsumer).IsExpired(execution.Round.ConsensusTime))
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

	public bool RequireDomainAccess(RdnExecution execution, AutoId id, out Domain domain)
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

	public bool RequireResourceAccess(RdnExecution execution, AutoId id, out Domain domain, out Resource resource)
	{
		if(!RequireResource(execution, id, out domain, out resource))
			return false; 

		if(!RequireDomainAccess(execution, resource.Domain, out _))
			return false; 

		return true; 
	}
}
