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

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequireSiteAccess(round, SiteId, out var a))
			return;
		
		a = round.AffectSite(SiteId);

		if(!Site.CanRenew(a, round.ConsensusTime))
		{
			Error = NotAvailable;
			return;
		}

		Prolong(round, Signer, a, Time.FromYears(Years));
	}
}
