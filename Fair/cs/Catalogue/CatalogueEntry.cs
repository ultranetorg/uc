namespace Uccs.Fair;

public class CatalogueEntry : Catalogue, ITableEntry
{
	public BaseId			BaseId => Id;
	public bool				Deleted { get; set; }
	FairMcv					Mcv;

	public CatalogueEntry()
	{
	}

	public CatalogueEntry(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public CatalogueEntry Clone()
	{
		return new(Mcv){Id = Id,
						Title = Title,
						Owners = [..Owners],
						Topics = [..Topics]};
	}

	public void ReadMain(BinaryReader reader)
	{
		Id		= reader.Read<EntityId>();
		Title	= reader.ReadUtf8();
		Owners	= reader.ReadArray<EntityId>();
		Topics	= reader.ReadArray<EntityId>();
	}

	public void WriteMain(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Title);
		writer.Write(Owners);
		writer.Write(Topics);
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

