namespace Uccs.Rdn;

public class DomainPolicyUpdation : RdnOperation
{
	public new AutoId			Id { get; set; }
	public OwnershipPolicy		Policy { get; set; }

	public override string		Explanation => $"{Id} {Policy}";
	
	public DomainPolicyUpdation()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		if(!Enum.IsDefined(Policy) || Policy == OwnershipPolicy.None)
			return false;

		return true;
	}

	public override void Read(Reader reader)
	{
		Id		= reader.Read<AutoId>();
		Policy	= reader.Read<OwnershipPolicy>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Id);
		writer.Write(Policy);
	}

	public override void Execute(RdnExecution execution)
	{
		var d = execution.Domains.Find(Id);

		if(d == null)
		{
			Error = NotFound;
			return;
		}

		if(!DomainName.IsRoot(d.Name))
		{
			if(!RequireDomainNameAccess(execution, DomainName.GetParent(d.Name), out var p))
				return;

			if(d.OwnershipPolicy == OwnershipPolicy.FullFreedom && !(d as ISpaceConsumer).IsExpired(execution.Time))
			{
				Error = NotAvailable;
				return;
			}

			d = execution.Domains.Affect(d.Id);
			d.OwnershipPolicy = Policy;
		}
		else
		{
			Error = NotAvailable;
			return;
		}

		execution.PayOperationEnergy(User);
	}
}
