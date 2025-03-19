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

	public override void Execute(RdnExecution execution)
	{
		var e = execution.FindDomain(Id);
		
		if(e == null)
		{
			Error = NotFound;
			return;
		}	

		if(Domain.IsRoot(e.Address))
		{
			if(!Domain.CanRegister(e.Address, e, execution.Time, Signer))
			{
				Error = NotAvailable;
				return;
			}

			e = execution.AffectDomain(e.Address);

			PayForName(e.Address, Years);
		} 
		else
		{
			if(!Domain.CanRenew(e, Signer, execution.Time))
			{
				Error = NotAvailable;
				return;
			}

			e = execution.AffectDomain(e.Address);

			PayForName(new string(' ', Domain.NameLengthMax), Years);
		}

		Prolong(execution, Signer, e, Time.FromYears(Years));
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

	public override void Execute(RdnExecution execution)
	{
		var e = execution.FindDomain(Id);
		
		if(e == null)
		{
			Error = NotFound;
			return;
		}	

		if(Domain.IsRoot(e.Address))
		{
			if(!Domain.IsOwner(e, Signer, execution.Time))
			{
				Error = Denied;
				return;
			}

			if(!RequireAccount(execution, Owner, out var o))
				return;

			e = execution.AffectDomain(e.Address);
			e.Owner = Owner;

		} 
		else
		{
			var p = execution.FindDomain(Domain.GetParent(e.Address));

			//if(p == null)
			//{
			//	Error = NotFound;
			//	return;
			//}

			if(e.ParentPolicy == DomainChildPolicy.FullOwnership && !Domain.IsOwner(p, Signer, execution.Time))
			{
				Error = Denied;
				return;
			}

			if(e.ParentPolicy == DomainChildPolicy.FullFreedom && !Domain.IsOwner(e, Signer, execution.Time) && 
																  !(Domain.IsOwner(p, Signer, execution.Time) && Domain.IsExpired(e, execution.Time)))
			{
				Error = Denied;
				return;
			}

			if(!RequireAccount(execution, Owner, out var o))
				return;

			e = execution.AffectDomain(e.Address);
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

	public override void Execute(RdnExecution execution)
	{
		var e = execution.FindDomain(Id);
		
		if(e == null)
		{
			Error = NotFound;
			return;
		}			

		if(!Domain.IsRoot(e.Address))
		{
			var p = execution.FindDomain(Domain.GetParent(e.Address));

			if(!Domain.IsOwner(p, Signer, execution.Time))
			{
				Error = Denied;
				return;
			}

			if(e.ParentPolicy == DomainChildPolicy.FullFreedom && !Domain.IsExpired(e, execution.Time))
			{
				Error = NotAvailable;
				return;
			}

			e = execution.AffectDomain(e.Address);
			e.ParentPolicy = Policy;
		}
		else
		{
			Error = NotAvailable;
			return;
		}
	}
}
