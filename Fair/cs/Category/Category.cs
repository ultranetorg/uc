using System.Collections.Immutable;

namespace Uccs.Fair;

public class Category : IBinarySerializable, ITableEntry
{
	public EntityId			Id { get; set; }
	public EntityId			Site { get; set; }
	public EntityId			Parent { get; set; }
	public string			Title { get; set; }
	public EntityId[]		Categories { get; set; }
	public EntityId[]		Publications { get; set; }

	public BaseId			Key => Id;
	public bool				Deleted { get; set; }
	FairMcv					Mcv;

	public Category()
	{
	}

	public Category(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public Category Clone()
	{
		return new(Mcv){Id			 = Id,
						Site		 = Site,
						Parent		 = Parent,
						Title		 = Title,
						Categories	 = Categories,
						Publications = Publications};
	}

	public void ReadMain(BinaryReader reader)
	{
		Read(reader);
	}

	public void WriteMain(BinaryWriter writer)
	{
		Write(writer);
	}

	public void Cleanup(Round lastInCommit)
	{
	}

	public void Read(BinaryReader reader)
	{
		Id				= reader.Read<EntityId>();
		Site			= reader.Read<EntityId>();
		Parent			= reader.ReadNullable<EntityId>();
		Title			= reader.ReadUtf8();
		Categories		= reader.ReadArray<EntityId>();
		Publications	= reader.ReadArray<EntityId>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Site);
		writer.WriteNullable(Parent);
		writer.WriteUtf8(Title);
		writer.Write(Categories);
		writer.Write(Publications);
	}
}
