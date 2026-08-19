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

		var n = execution.DomainNames.Find(Name);

		if(n != null && execution.Time.Days <= n.Expiration)
		{
			Error = NotAvailable;
			return;
		}

		n = (n == null) ? execution.DomainNames.Create(Name) : execution.DomainNames.Affect(n.Id);

		n.Owner = User.Id;
		
		User.DomainNames = User.DomainNames.Add(n.Id);

		execution.PayForName(User, n, Years);
		execution.PayOperationEnergy(User);
	}
}
