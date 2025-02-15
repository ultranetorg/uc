﻿namespace Uccs.Rdn;

public enum RdnOperationClass
{
	RdnCandidacyDeclaration		= OperationClass.CandidacyDeclaration, 

	Domain						= 100,
		DomainRegistration		= 100_000_001, 
		DomainMigration			= 100_000_002, 
		DomainBid				= 100_000_003, 
		DomainUpdation			= 100_000_004,

	Resource					= 101,
		ResourceCreation		= 101_000_001, 
		ResourceUpdation		= 101_000_002, 
		ResourceDeletion		= 101_000_003, 
		ResourceLinkCreation	= 101_000_004, 
		ResourceLinkDeletion	= 101_000_005,

	Analysis					= 102,
		AnalysisResultUpdation	= 102_000_001
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

	public void PayForName(string address, int years)
	{
		var fee = NameFee(years, address);
		
		Signer.Spacetime -= fee;
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
	}

	public bool RequireDomain(RdnRound round, EntityId id, out DomainEntry domain)
	{
		domain = round.Mcv.Domains.Find(id, round.Id);

		if(domain == null || domain.Deleted)
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

	public bool RequireDomain(RdnRound round, string name, out DomainEntry domain)
	{
		domain = round.Mcv.Domains.Find(name, round.Id);

		if(domain == null || domain.Deleted)
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

	public bool RequireSignerDomain(RdnRound round, string name, out DomainEntry domain)
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

	public bool RequireSignerDomain(RdnRound round, EntityId id, out DomainEntry domain)
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

	public bool RequireResource(RdnRound round, EntityId id, out DomainEntry domain, out ResourceEntry resource)
	{
		resource = round.Mcv.Resources.Find(id, round.Id);

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

	public bool RequireSignerResource(RdnRound round, EntityId id, out DomainEntry domain, out ResourceEntry resource)
	{
		if(!RequireResource(round, id, out domain, out resource))
			return false; 

		if(!RequireSignerDomain(round, resource.Domain, out _))
			return false; 

		return true; 
	}
}
