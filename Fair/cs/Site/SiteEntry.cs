namespace Uccs.Fair;

public class SiteEntry : Site, ITableEntry
{
	public bool				Deleted { get; set; }
	FairMcv					Mcv;

	public SiteEntry()
	{
	}

	public SiteEntry(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public SiteEntry Clone()
	{
		var a = new SiteEntry(Mcv){	Id						= Id,
									Title					= Title,
									ModerationReward		= ModerationReward,
									
									ChangePolicies			= ChangePolicies,

									Expiration				= Expiration,
									Space					= Space,
									Spacetime				= Spacetime,

									Authors					= Authors,
									Moderators				= Moderators,
									Categories				= Categories,
									Disputes				= Disputes,
									};
		
		((IEnergyHolder)this).Clone(a);

		return a;
	}

	public void ReadMain(BinaryReader reader)
	{
		Read(reader);
	}

	public void WriteMain(BinaryWriter writer)
	{
		Write(writer);
	}

	public void WriteMore(BinaryWriter w)
	{
	}

	public void ReadMore(BinaryReader r)
	{
	}

	public void Cleanup(Round lastInCommit)
	{
	}

}

