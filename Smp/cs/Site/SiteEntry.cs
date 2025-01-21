namespace Uccs.Smp;

public class SiteEntry : Site, ITableEntry
{
	public BaseId			BaseId => Id;
	public bool				Deleted { get; set; }
	SmpMcv					Mcv;

	public SiteEntry()
	{
	}

	public SiteEntry(SmpMcv mcv)
	{
		Mcv = mcv;
	}

	public SiteEntry Clone()
	{
		return new(Mcv){Id = Id,
						Type = Type,
						Title = Title,
						Owners = Owners,
						Categories = Categories};
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

