namespace Uccs.Fair;

public enum EntityTextField : byte
{
	AccountNickname, 

	AuthorNickname, 
	AuthorTitle,

	SiteNickname, 
	SiteTitle,

	ProductNickname, 
	PublicationTitle,
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

public class Text : IBinarySerializable, ITableEntry
{
	public StringId			Id { get; set; }
	public TextField[]		Entities { get; set; }

	public BaseId			Key => Id;
	public bool				Deleted { get; set; }
	FairMcv					Mcv;

	public Text()
	{
	}

	public Text(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public Text Clone()
	{
		var a = new Text(Mcv){	Id			= Id,
								Entities	= Entities
								};

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

	public void Cleanup(Round lastInCommit)
	{
	}

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
