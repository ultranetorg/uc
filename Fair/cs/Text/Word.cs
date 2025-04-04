using System.Text;

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

public class WordReference : IBinarySerializable
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

public class Word : IBinarySerializable, ITableEntry
{
	public RawId			Id { get; set; }
	public WordReference[]	References { get; set; }

	public BaseId			Key => Id;
	public bool				Deleted { get; set; }
	FairMcv					Mcv;


	public Word()
	{
	}

	public Word(FairMcv mcv)
	{
		Mcv = mcv;
	}
	
	public static RawId	GetId(string t)
	{
		var b = Encoding.UTF8.GetBytes(t);

		return new RawId(b);
	}

	public Word Clone()
	{
		var a = new Word(Mcv)  {Id			= Id,
								References	= References};

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
		Id			= reader.Read<RawId>();
		References	= reader.ReadArray<WordReference>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(References);
	}
}
