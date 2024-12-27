namespace Uccs.Fair;

public class TopicEntry : Topic, ITableEntry
{
	public BaseId		BaseId => Id;
	public bool			Deleted { get; set; }
	FairMcv				Mcv;

	public TopicEntry()
	{
	}

	public TopicEntry(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public TopicEntry Clone()
	{
		return new(Mcv){Id = Id,
						Catalogue = Catalogue,
						Product	= Product,
						Reviews	= [..Reviews]};
	}

	public void ReadMain(BinaryReader reader)
	{
		Id			= reader.Read<EntityId>();
		Catalogue	= reader.Read<EntityId>();
		Product		= reader.Read<EntityId>();
		Reviews		= reader.ReadArray<Review>();
	}

	public void WriteMain(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Catalogue);
		writer.Write(Product);
		writer.Write(Reviews);
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

