using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uccs.Net
{
	public abstract class RdsOperation : Operation
	{
		public abstract void Execute(Rds mcv, RdsRound round);

		public override void Execute(Mcv mcv, Round round)
		{
			Execute(mcv as Rds, round as RdsRound);
		}

		public bool RequireDomain(RdsRound round, AccountAddress signer, string name, out DomainEntry domain)
		{
			domain = round.Rds.Domains.Find(name, round.Id);

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

			if(signer != null && domain.Owner != signer)
			{
				Error = NotOwner;
				return false;
			}

			return true;
		}

		public bool RequireDomain(RdsRound round, AccountAddress signer, EntityId id, out DomainEntry domain)
		{
			domain = round.Rds.Domains.Find(id, round.Id);

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

			if(signer != null && domain.Owner != signer)
			{
				Error = NotOwner;
				return false;
			}

			return true;
		}

		public bool Require(RdsRound round, AccountAddress signer, ResourceId id, out DomainEntry domain, out Resource resource)
		{
			resource = null;

			if(RequireDomain(round, signer, id.DomainId, out domain) == false)
			{
				Error = NotFound;
				return false; 
			}

			resource = domain.Resources.FirstOrDefault(i => i.Id == id);
			
			if(resource == null)
			{
				Error = NotFound;
				return false; 
			}

			return true; 
		}
	}
}
