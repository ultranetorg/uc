namespace Uccs.Fair;

class SiteRenewal : FairOperation
{
	public AutoId				SiteId { get; set; }
	public byte					Years { get; set; }

	public override string		Explanation => $"{SiteId}, {Years}";
	
	public SiteRenewal()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		if((Years < Mcv.EntityRentYearsMin || Years > Mcv.EntityRentYearsMax))
			return false;

		return true;
	}

	public override void Read(BinaryReader reader)
	{
		SiteId	= reader.Read<AutoId>();
		Years	= reader.ReadByte();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(SiteId);
		writer.Write(Years);
	}

	public override void Execute(FairExecution execution)
	{
		if(!IsModerator(execution, SiteId, out var s, out Error))
			return;
		
		s = execution.Sites.Affect(SiteId);

		if(!Site.CanRenew(s, execution.Time))
		{
			Error = NotAvailable;
			return;
		}

		execution.Prolong(s, s, Time.FromYears(Years));
	}
}
