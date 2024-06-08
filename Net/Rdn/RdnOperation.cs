using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uccs.Net
{
	public abstract class RdnOperation : Operation
	{
		public abstract void Execute(Rdn mcv, RdnRound round);

		public override void Execute(Mcv mcv, Round round)
		{
			Execute(mcv as Rdn, round as RdnRound);
		}

		public bool RequireDomain(RdnRound round, AccountAddress signer, string name, out DomainEntry domain)
		{
			domain = round.Rdn.Domains.Find(name, round.Id);

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

		public bool RequireDomain(RdnRound round, AccountAddress signer, EntityId id, out DomainEntry domain)
		{
			domain = round.Rdn.Domains.Find(id, round.Id);

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

		public bool Require(RdnRound round, AccountAddress signer, ResourceId id, out DomainEntry domain, out Resource resource)
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
