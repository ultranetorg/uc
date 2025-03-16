namespace Uccs.Rdn;

public class DomainRenewal : RdnOperation
{
	public new EntityId			Id {get; set;}
	public byte					Years  {get; set;}

	public override string		Description => $"{Id} {Years}";
	
	public DomainRenewal()
	{
	}
	
	public override bool IsValid(Mcv mcv)
	{ 
		if(Years < Mcv.EntityRentYearsMin || Years > Mcv.EntityRentYearsMax)
			return false;

		return true;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Id		= reader.Read<EntityId>();
		Years	= reader.ReadByte();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Years);
	}

	public override void Execute(RdnMcv mcv, RdnRound round)
	{
		var e = mcv.Domains.Find(Id, round.Id);
		
		if(e == null)
		{
			Error = NotFound;
			return;
		}	

		if(Domain.IsRoot(e.Address))
		{
			if(!Domain.CanRegister(e.Address, e, round.ConsensusTime, Signer))
			{
				Error = NotAvailable;
				return;
			}

			e = round.AffectDomain(e.Address);

			PayForName(e.Address, Years);
		} 
		else
		{
			if(!Domain.CanRenew(e, Signer, round.ConsensusTime))
			{
				Error = NotAvailable;
				return;
			}

			e = round.AffectDomain(e.Address);

			PayForName(new string(' ', Domain.NameLengthMax), Years);
		}

		Prolong(round, Signer, e, Time.FromYears(Years));
	}
}

public class DomainTransfer : RdnOperation
{
	public new EntityId			Id {get; set;}
	public EntityId				Owner  {get; set;}

	public override string		Description => $"{Id} {Owner}";
	
	public DomainTransfer()
	{
	}
	
	public override bool IsValid(Mcv mcv)
	{ 
		return true;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Id		= reader.Read<EntityId>();
		Owner	= reader.Read<EntityId>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Owner);
	}

	public override void Execute(RdnMcv mcv, RdnRound round)
	{
		var e = mcv.Domains.Find(Id, round.Id);
		
		if(e == null)
		{
			Error = NotFound;
			return;
		}	

		if(Domain.IsRoot(e.Address))
		{
			if(!Domain.IsOwner(e, Signer, round.ConsensusTime))
			{
				Error = Denied;
				return;
			}

			if(!RequireAccount(round, Owner, out var o))
				return;

			e = round.AffectDomain(e.Address);
			e.Owner = Owner;

		} 
		else
		{
			var p = mcv.Domains.Find(Domain.GetParent(e.Address), round.Id);

			//if(p == null)
			//{
			//	Error = NotFound;
			//	return;
			//}

			if(e.ParentPolicy == DomainChildPolicy.FullOwnership && !Domain.IsOwner(p, Signer, round.ConsensusTime))
			{
				Error = Denied;
				return;
			}

			if(e.ParentPolicy == DomainChildPolicy.FullFreedom && !Domain.IsOwner(e, Signer, round.ConsensusTime) && 
																  !(Domain.IsOwner(p, Signer, round.ConsensusTime) && Domain.IsExpired(e, round.ConsensusTime)))
			{
				Error = Denied;
				return;
			}

			if(!RequireAccount(round, Owner, out var o))
				return;

			e = round.AffectDomain(e.Address);
			e.Owner	= Owner;
		}
	}
}

public class DomainPolicyUpdation : RdnOperation
{
	public new EntityId			Id { get; set; }
	public DomainChildPolicy	Policy { get; set; }

	public override string		Description => $"{Id} {Policy}";
	
	public DomainPolicyUpdation()
	{
	}
	
	public override bool IsValid(Mcv mcv)
	{ 
		if(!Enum.IsDefined(Policy) || Policy == DomainChildPolicy.None)
			return false;

		return true;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Id		= reader.Read<EntityId>();
		Policy	= reader.Read<DomainChildPolicy>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Policy);
	}

	public override void Execute(RdnMcv mcv, RdnRound round)
	{
		var e = mcv.Domains.Find(Id, round.Id);
		
		if(e == null)
		{
			Error = NotFound;
			return;
		}			

		if(!Domain.IsRoot(e.Address))
		{
			var p = mcv.Domains.Find(Domain.GetParent(e.Address), round.Id);

			if(!Domain.IsOwner(p, Signer, round.ConsensusTime))
			{
				Error = Denied;
				return;
			}

			if(e.ParentPolicy == DomainChildPolicy.FullFreedom && !Domain.IsExpired(e, round.ConsensusTime))
			{
				Error = NotAvailable;
				return;
			}

			e = round.AffectDomain(e.Address);
			e.ParentPolicy = Policy;
		}
		else
		{
			Error = NotAvailable;
			return;
		}
	}
}
