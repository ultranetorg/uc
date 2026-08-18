namespace Uccs.Rdn;

public class DomainNameAcquisition : RdnOperation
{
	public string				Name {get; set;}
	public byte					Years {get; set;}

	public override string		Explanation => $"{Name} for {Years} years";
	
	public DomainNameAcquisition()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return	DomainName.IsAddressValid(Name) &&
				IsRentTimeValid(Years) &&
				!DomainNameTable.Priority.Any(i => i.Value.Contains(Name));
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
		if(User.Key == null)
		{
			Error = NotAllowedForNewUser;
			return;
		}

		if(DomainName.IsSubdomain(Name))
			if(!RequireDomainNameAccess(execution, DomainName.GetParent(Name), out var p))
				return;

		var a = execution.DomainNames.Find(Name);

		if(a != null && execution.Time.Days <= a.Expiration)
		{
			Error = NotAvailable;
			return;
		}

		a ??= execution.DomainNames.Create(Name);

		a.Owner = User.Id;
		
		User.DomainNames = User.DomainNames.Add(new StringId(Name));

		execution.PayForName(User, a, Years);
		execution.PayOperationEnergy(User);
	}
}
