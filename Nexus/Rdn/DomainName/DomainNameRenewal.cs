namespace Uccs.Rdn;

public class DomainNameRenewal : RdnOperation
{
	public string				Name {get; set;}
	public byte					Years  {get; set;}

	public override string		Explanation => $"{Id} {Years}";
	
	public DomainNameRenewal()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return	IsRentTimeValid(Years) &&
				DomainName.IsRoot(Name);
	}

	public override void Read(Reader reader)
	{
		Name	= reader.ReadASCII();
		Years	= reader.ReadByte();
	}

	public override void Write(Writer writer)
	{
		writer.WriteASCII(Name);
		writer.Write(Years);
	}

	public override void Execute(RdnExecution execution)
	{
		if(!RequireDomainNameAccess(execution, Name, out var a))
			return;

		if(!(a as IExpirable).CanRenew(execution.Time, Time.FromYears(Years)))
		{
			Error = NotAvailable;
			return;
		}
	
		a = execution.DomainNames.Affect(a.Id);

		execution.PayForName(User, a, Years);
		execution.PayOperationEnergy(User);
	}
}