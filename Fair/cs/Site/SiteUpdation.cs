﻿namespace Uccs.Fair;

public enum SiteChange : byte
{
	None, Renew
}

public class SiteUpdation : UpdateOperation
{
	public EntityId				SiteId { get; set; }
	public SiteChange			Change { get; set; }
	public object				Second { get; set; }

	public override string		Description => $"{SiteId}, {Change}={Value}";
	
	public SiteUpdation()
	{
	}
	
	public override bool IsValid(Mcv mcv)
	{ 
		if(!Enum.IsDefined(Change) || Change == SiteChange.None) 
			return false;
		
		if(Change == SiteChange.Renew && (Byte < Mcv.EntityRentYearsMin || Byte > Mcv.EntityRentYearsMax))
			return false;

		return true;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		SiteId	= reader.Read<EntityId>();
		Change	= reader.ReadEnum<SiteChange>();

		Value = Change switch
					   {
							SiteChange.Renew				=> reader.ReadByte(),
							_								=> throw new IntegrityException()
					   };
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(SiteId);
		writer.WriteEnum(Change);

		switch(Change)
		{
			case SiteChange.Renew				: writer.Write(Byte); break;
			default								: throw new IntegrityException();
		}
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequireSiteAccess(round, SiteId, out var a))
			return;
		
		a = round.AffectSite(SiteId);

		switch(Change)
		{
			case SiteChange.Renew:
			{	
				if(!Site.CanRenew(a, round.ConsensusTime))
				{
					Error = NotAvailable;
					return;
				}

				Prolong(round, Signer, a, Time.FromYears(Byte));

				break;
			}
		}
	}
}
