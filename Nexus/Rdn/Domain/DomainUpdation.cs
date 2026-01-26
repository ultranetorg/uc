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
		var d = execution.Domains.Find(Id);
		
		if(d == null)
		{
			Error = NotFound;
			return;
		}	

		if(!d.CanRenew(User, execution.Time, Time.FromYears(Years)))
		{
			Error = NotAvailable;
			return;
		}

		if(execution.Net.IsFree(d) && Years > 1)
		{
			Error = NotAvailable;
			return;
		}
	
		d = execution.Domains.Affect(d.Address);

		if(Domain.IsRoot(d.Address))
		{
			if(!execution.Net.IsFree(d))
				execution.PayForName(d.Address, Years);
		} 
		else
		{
			execution.PayForName(new string(' ', Domain.NameLengthMax), Years);
		}

		execution.Prolong(User, d, Time.FromYears(Years));
		execution.PayOperationEnergy(User);
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

		if(!AccountExists(execution, Owner, out var o, out Error))
			return;

		if(Domain.IsRoot(e.Address))
		{
			if(!Domain.IsOwner(e, User, execution.Time))
			{
				Error = Denied;
				return;
			}

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

			if(e.ParentPolicy == DomainChildPolicy.FullOwnership && !Domain.IsOwner(p, User, execution.Time))
			{
				Error = Denied;
				return;
			}

			if(e.ParentPolicy == DomainChildPolicy.FullFreedom && (!Domain.IsOwner(e, User, execution.Time) || e.IsExpired(execution.Time)))
			{
				Error = Denied;
				return;
			}

			e = execution.Domains.Affect(e.Address);
			e.Owner	= Owner;
		}

		execution.PayOperationEnergy(User);
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

			if(!Domain.IsOwner(p, User, execution.Time))
			{
				Error = Denied;
				return;
			}

			if(e.ParentPolicy == DomainChildPolicy.FullFreedom && !e.IsExpired(execution.Time))
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

		execution.PayOperationEnergy(User);
	}
}
