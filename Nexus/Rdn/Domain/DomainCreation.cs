namespace Uccs.Rdn;

public class DomainCreation : RdnOperation
{
	public string				Name {get; set;}
	public byte					Years {get; set;}
	public AutoId				Owner  {get; set;}
	public OwnershipPolicy		Policy {get; set;}

	public override string		Explanation => $"{Name} for {Years} years, {nameof(Owner)}={Owner}, {nameof(Policy)}={Policy}";
	
	public DomainCreation()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return	DomainName.IsAddressValid(Name) &&
				IsRentTimeValid(Years) &&
				(DomainName.IsRoot(Name) || (Owner != null && Enum.IsDefined(Policy)));
	}

	public override void Read(Reader reader)
	{
		Name	= reader.ReadASCII();
		Years = reader.ReadByte();

		if(DomainName.IsSubdomain(Name))
		{
			Owner = reader.Read<AutoId>();
			Policy	= reader.Read<OwnershipPolicy>();
		}
	}

	public override void Write(Writer writer)
	{
		writer.WriteASCII(Name);
		writer.Write(Years);

		if(DomainName.IsSubdomain(Name))
		{
			writer.Write(Owner);
			writer.Write(Policy);
		}
	}

	public override void Execute(RdnExecution execution)
	{
		if(User.Key == null)
		{
			Error = NotAllowedForNewUser;
			return;
		}

		if(!RequireDomainNameAccess(execution, Name, out var a))
			return;
	
		if(a.Domain != null)
		{
			Error = NotAvailable;
			return;
		}

		Domain d;
		a = execution.DomainNames.Affect(a.Id);

		if(DomainName.IsRoot(Name))
		{
			d = execution.Domains.Create();

			d.Name	= Name;
			d.Owner	= User.Id;
			///d.Free		= Address.Length >= execution.Net.FreeNameLengthMinimum && Years == 1;
			
			execution.Prolong(User, d, Time.FromYears(Years));
			
			a.Domain = d.Id;
		}
		else
		{
			if(!UserExists(execution, Owner, out var o, out Error))
				return;
//
//			if(!RequireDomainAccess(execution, DomainName.GetParent(Name), out var p))
//				return;

			d = execution.Domains.Create();
			
			var start = Math.Max(execution.Time.Days, d.Expiration);

			d.Name				= Name;
			d.Owner				= o.Id;
			d.OwnershipPolicy	= Policy;
			d.Expiration		= (short)(start + Time.FromYears(Years).Days);

			a.Domain = d.Id;
		}

		//execution.Allocate(User, User, execution.Net.EntityLength);
		execution.PayOperationEnergy(User);
	}
}
