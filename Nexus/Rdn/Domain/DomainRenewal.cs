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
		return IsRentTimeValid(Years);
	}

	public override void Read(Reader reader)
	{
		Id		= reader.Read<AutoId>();
		Years	= reader.ReadByte();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Id);
		writer.Write(Years);
	}

	public override void Execute(RdnExecution execution)
	{
		if(!RequireDomainAccess(execution, Id, out var d))
			return;

		if(!(d as ISpaceConsumer).CanRenew(execution.Time, Time.FromYears(Years)))
		{
			Error = TooLongDuration;
			return;
		}
	
		d = execution.Domains.Affect(d.Id);

		execution.Prolong(User, d, Time.FromYears(Years));
		execution.PayOperationEnergy(User);
	}
}
