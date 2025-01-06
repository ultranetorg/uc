namespace Uccs.Fair;

public class PageEntry : Page, ITableEntry
{
	public BaseId		BaseId => Id;
	public PageField	Affecteds;
	public bool			Deleted { get; set; }
	FairMcv				Mcv;

	public PageEntry()
	{
	}

	public PageEntry(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public PageEntry Clone()
	{
		return new(Mcv){Id			= Id,
						Site		= Site,
						Fields		= Fields,
						Permissions = Permissions,
						Content		= Content,
						Pages		= Pages,
						Comments	= Comments};
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

