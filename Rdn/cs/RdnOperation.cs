namespace Uccs.Rdn
{
	public abstract class RdnOperation : Operation
	{
		public abstract void Execute(RdnMcv mcv, RdnRound round);

		public override void Execute(Mcv mcv, Round round)
		{
			Execute(mcv as RdnMcv, round as RdnRound);
		}

		public void PayForSpacetime(Round round, int length, Time time)
		{
			var fee = SpacetimeFee(length, time);
			
			round.AffectAccount(Signer).STBalance -= fee;
		}

		public static Money SpacetimeFee(int length, Time time)
		{
			return length * Mcv.TimeFactor(time);
		}

		public static Money NameFee(int years, string address)
		{
			var l = Domain.IsWeb(address) ? address.Length : (address.Length - Domain.NormalPrefix.ToString().Length);

			l = Math.Min(l, 10);

			return Mcv.TimeFactor(Time.FromYears(years)) * 10_000/(l * l * l * l);
		}

		public void Allocate(Round round, Domain domain, int toallocate)
		{
			if(domain.SpaceReserved < domain.SpaceUsed + toallocate)
			{
				PayForSpacetime(round, domain.SpaceUsed + toallocate - domain.SpaceReserved, domain.Expiration - round.ConsensusTime);
	
				domain.SpaceReserved = 
				domain.SpaceUsed = (short)(domain.SpaceUsed + toallocate);
			}
			else
				domain.SpaceUsed += (short)toallocate;
		}

		public void Free(Domain domain, int toallocate)
		{
			domain.SpaceUsed -= (short)toallocate;
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
