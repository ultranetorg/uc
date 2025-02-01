using System.Collections.Immutable;

namespace Uccs.Fair;

public class Category : IBinarySerializable
{
	public EntityId			Id { get; set; }
	public EntityId			Site { get; set; }
	public EntityId			Parent { get; set; }
	public string			Title { get; set; }
	public EntityId[]		Categories { get; set; }
	public EntityId[]		Publications { get; set; }

	//[Flags]
	//enum Field
	//{
	//	None,
	//	Security	= 1
	//}

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
