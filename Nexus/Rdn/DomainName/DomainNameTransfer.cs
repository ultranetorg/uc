namespace Uccs.Rdn;

public class DomainNameTransfer : RdnOperation
{
	public string				Name {get; set;}
	public AutoId				Owner {get; set;}

	public override string		Explanation => $"{Name} {Owner}";
	
	public DomainNameTransfer()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(Reader reader)
	{
		Name	= reader.ReadASCII();
		Owner	= reader.Read<AutoId>();
	}

	public override void Write(Writer writer)
	{
		writer.WriteASCII(Name);
		writer.Write(Owner);
	}

	public override void Execute(RdnExecution execution)
	{
		if(!RequireDomainNameAccess(execution, Name, out var d))
			return;

		if(!UserExists(execution, Owner, out var o, out Error))
			return;

		d = execution.DomainNames.Affect(d.Id);
		d.Owner = Owner;

		execution.PayOperationEnergy(User);
	}
}
