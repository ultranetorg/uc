namespace Uccs.Fair;

class SiteRewal : FairOperation
{
	public EntityId				SiteId { get; set; }
	public byte					Years { get; set; }

	public override string		Description => $"{SiteId}, {Years}";
	
	public SiteRewal()
	{
	}
	
	public override bool IsValid(Mcv mcv)
	{ 
		if((Years < Mcv.EntityRentYearsMin || Years > Mcv.EntityRentYearsMax))
			return false;

		return true;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		SiteId	= reader.Read<EntityId>();
		Years	= reader.ReadByte();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(SiteId);
		writer.Write(Years);
	}

	public override void Execute(FairExecution execution)
	{
		if(!RequireSiteAccess(execution, SiteId, out var a))
			return;
		
		a = execution.AffectSite(SiteId);

		if(!Site.CanRenew(a, execution.Time))
		{
			Error = NotAvailable;
			return;
		}

		Prolong(execution, Signer, a, Time.FromYears(Years));
	}
}
