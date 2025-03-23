namespace Uccs.Fair;

public enum EntityTextField : byte
{
	AccountNickname, 

	AuthorNickname, 
	AuthorTitle,

	SiteNickname, 
	SiteTitle,

	ProductNickname, 
	ProductTitle,
}

public class TextField : IBinarySerializable
{
	public EntityTextField		Field { get; set; }
	public EntityId				Entity { get; set; }

	public void Read(BinaryReader reader)
	{
		Field	= reader.Read<EntityTextField>();
		Entity	= reader.Read<EntityId>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Field);
		writer.Write(Entity);
	}
}

public class Text : IBinarySerializable
{
	public StringId			Id { get; set; }
	public TextField[]		Entities { get; set; }

	public void Read(BinaryReader reader)
	{
		Id			= reader.Read<StringId>();
		Entities	= reader.ReadArray<TextField>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Entities);
	}
}
