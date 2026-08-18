namespace Uccs.Rdn;

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

	public override void Read(Reader reader)
	{
		Id		= reader.Read<AutoId>();
		Owner	= reader.Read<AutoId>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Id);
		writer.Write(Owner);
	}

	public override void Execute(RdnExecution execution)
	{
		var d = execution.Domains.Find(Id);

		if(d == null)
		{
			Error = NotFound;
			return;
		}

		if(!UserExists(execution, Owner, out var o, out Error))
			return;

		if(DomainName.IsRoot(d.Name))
		{
			if(!RequireDomainAccess(execution, Id, out var _))
				return;

			d = execution.Domains.Affect(d.Id);
			d.Owner = Owner;
		} 
		else
		{
			var p = execution.DomainNames.Find(DomainName.GetParent(d.Name));

			if(d.OwnershipPolicy == OwnershipPolicy.FullOwnership && p.Owner != User.Id)
			{
				Error = Denied;
				return;
			}

			if(d.OwnershipPolicy == OwnershipPolicy.FullFreedom && (d.Owner != User.Id || (d as ISpaceConsumer).IsExpired(execution.Time)))
			{
				Error = Denied;
				return;
			}

			d = execution.Domains.Affect(d.Id);
			d.Owner	= Owner;
		}

		execution.PayOperationEnergy(User);
	}
}
