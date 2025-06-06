﻿namespace Uccs.Fair;

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

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!RequireModeratorAccess(execution, SiteId, out var a))
			return;
		
		a = execution.Sites.Affect(SiteId);

		if(!Site.CanRenew(a, execution.Time))
		{
			Error = NotAvailable;
			return;
		}

		Prolong(execution, Signer, a, Time.FromYears(Years));
	}
}
