namespace Uccs.Rdn;

public class DomainRegistration : RdnOperation
{
	public string				Address {get; set;}
	public byte					Years {get; set;}
	public AutoId				Owner  {get; set;}
	public DomainChildPolicy	Policy {get; set;}

	public override string		Explanation => $"{Address} for {Years} years";
	
	public DomainRegistration()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		if(!Domain.Valid(Address))
			return false;
		
		if(Years < Mcv.EntityRentYearsMin || Years > Mcv.EntityRentYearsMax)
			return false;

		if(Domain.IsChild(Address) && (Owner == null || !Enum.IsDefined(Policy)))
			return false;

		return true;
	}

	public override void Read(BinaryReader reader)
	{
		Address	= reader.ReadUtf8();
		Years = reader.ReadByte();

		if(Domain.IsChild(Address))
		{
			Owner = reader.Read<AutoId>();
			Policy	= reader.Read<DomainChildPolicy>();
		}
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteUtf8(Address);
		writer.Write(Years);

		if(Domain.IsChild(Address))
		{
			writer.Write(Owner);
			writer.Write(Policy);
		}
	}

	public override void Execute(RdnExecution execution)
	{
		var d = execution.Domains.Find(Address);

		if(Domain.IsRoot(Address))
		{
			if(!Domain.CanRegister(Address, d, execution.Time, User))
			{
				Error = NotAvailable;
				return;
			}

			d = execution.Domains.Affect(Address);

			if(execution.Net.IsFree(d) && Years > 1)
			{
				Error = NotAvailable;
				return;
			}
			
			execution.Prolong(User, d, Time.FromYears(Years));
			
			if(!execution.Net.IsFree(d))
				execution.PayForName(Address, Years);
							
			d.Owner	= User.Id;
		}
		else
		{
			if(d != null)
			{
				Error = AlreadyExists;
				return;
			}

			var o = execution.FindUser(Owner);

			if(o == null)
			{
				Error = NotFound;
				return;
			}

			if(!RequireDomainAccess(execution, Domain.GetParent(Address), out var p))
				return;

			if(Policy < DomainChildPolicy.FullOwnership || DomainChildPolicy.FullFreedom < Policy)
			{
				Error = NotAvailable;
				return;
			}

			d = execution.Domains.Affect(Address);
			
			var start = d.Expiration < execution.Time.Days ? execution.Time.Days : d.Expiration;

			d.Owner			= o.Id;
			d.ParentPolicy	= Policy;
			d.Expiration	= (short)(start + Time.FromYears(Years).Days);

			execution.PayForName(new string(' ', Domain.NameLengthMax), Years);
		}

		execution.PayOperationEnergy(User);
	}
}
