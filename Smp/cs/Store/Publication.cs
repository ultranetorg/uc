using System.Collections.Immutable;

namespace Uccs.Smp;

public class Publication : IBinarySerializable
{
	public EntityId			Id { get; set; }
	public EntityId			Category { get; set; }
	public EntityId			Creator { get; set; }
	public EntityId			Product { get; set; }
	public string[]			Sections { get; set; }
	public EntityId[]		Comments { get; set; }

	//[Flags]
	//enum Field
	//{
	//	None,
	//	Security	= 1
	//}

	public void Read(BinaryReader reader)
	{
		Id			= reader.Read<EntityId>();
		Category	= reader.Read<EntityId>();
		Creator		= reader.Read<EntityId>();
		Product		= reader.Read<EntityId>();
		Sections	= reader.ReadArray(reader.ReadUtf8);
		Comments	= reader.ReadArray<EntityId>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Category);
		writer.Write(Creator);
		writer.Write(Product);
		writer.Write(Sections, writer.WriteUtf8);
		writer.Write(Comments);
	}
}
