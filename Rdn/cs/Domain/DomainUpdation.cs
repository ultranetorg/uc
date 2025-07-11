namespace Uccs.Rdn;

public class DomainRenewal : RdnOperation
{
	public new AutoId			Id {get; set;}
	public byte					Years  {get; set;}

	public override string		Explanation => $"{Id} {Years}";
	
	public DomainRenewal()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		if(Years < Mcv.EntityRentYearsMin || Years > Mcv.EntityRentYearsMax)
			return false;

		return true;
	}

	public override void Read(BinaryReader reader)
	{
		Id		= reader.Read<AutoId>();
		Years	= reader.ReadByte();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Years);
	}

	public override void Execute(RdnExecution execution)
	{
		var e = execution.Domains.Find(Id);
		
		if(e == null)
		{
			Error = NotFound;
			return;
		}	

		if(!Domain.CanRenew(e, Signer, execution.Time, Time.FromYears(Years)))
		{
			Error = NotAvailable;
			return;
		}
	
		e = execution.Domains.Affect(e.Address);

		if(Domain.IsRoot(e.Address))
		{
			execution.PayForName(e.Address, Years);
		} 
		else
		{
			execution.PayForName(new string(' ', Domain.NameLengthMax), Years);
		}

		execution.Prolong(Signer, e, Time.FromYears(Years));
	}
}

public class DomainTransfer : RdnOperation
{
	public new AutoId			Id {get; set;}
	public AutoId				Owner  {get; set;}

	public override string		Explanation => $"{Id} {Owner}";
	
	public DomainTransfer()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(BinaryReader reader)
	{
		Id		= reader.Read<AutoId>();
		Owner	= reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Owner);
	}

	public override void Execute(RdnExecution execution)
	{
		var e = execution.Domains.Find(Id);
		
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

			if(!CanAccessAccount(execution, Owner, out var o, out Error))
				return;

			e = execution.Domains.Affect(e.Address);
			e.Owner = Owner;

		} 
		else
		{
			var p = execution.Domains.Find(Domain.GetParent(e.Address));

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

			if(!CanAccessAccount(execution, Owner, out var o, out Error))
				return;

			e = execution.Domains.Affect(e.Address);
			e.Owner	= Owner;
		}
	}
}

public class DomainPolicyUpdation : RdnOperation
{
	public new AutoId			Id { get; set; }
	public DomainChildPolicy	Policy { get; set; }

	public override string		Explanation => $"{Id} {Policy}";
	
	public DomainPolicyUpdation()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		if(!Enum.IsDefined(Policy) || Policy == DomainChildPolicy.None)
			return false;

		return true;
	}

	public override void Read(BinaryReader reader)
	{
		Id		= reader.Read<AutoId>();
		Policy	= reader.Read<DomainChildPolicy>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Policy);
	}

	public override void Execute(RdnExecution execution)
	{
		var e = execution.Domains.Find(Id);
		
		if(e == null)
		{
			Error = NotFound;
			return;
		}			

		if(!Domain.IsRoot(e.Address))
		{
			var p = execution.Domains.Find(Domain.GetParent(e.Address));

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

			e = execution.Domains.Affect(e.Address);
			e.ParentPolicy = Policy;
		}
		else
		{
			Error = NotAvailable;
			return;
		}
	}
}
