namespace Uccs.Fair;

public class SiteEntry : Site, ITableEntry
{
	public BaseId			BaseId => Id;
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
		return new(Mcv){Id = Id,
						Title = Title,
						Owners = [..Owners],
						Roots = [..Roots]};
	}

	public void ReadMain(BinaryReader reader)
	{
		Id		= reader.Read<EntityId>();
		Title	= reader.ReadUtf8();
		Owners	= reader.ReadArray<EntityId>();
		Roots	= reader.ReadArray<EntityId>();
	}

	public void WriteMain(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Title);
		writer.Write(Owners);
		writer.Write(Roots);
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

