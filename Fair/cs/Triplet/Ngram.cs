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

public class TextReference : IBinarySerializable
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

public class Ngram : IBinarySerializable, ITableEntry
{
	public RawId			Id { get; set; }
	public RawId[]			Ngrams { get; set; }
	public TextReference[]	References { get; set; }

	public BaseId			Key => Id;
	public bool				Deleted { get; set; }
	FairMcv					Mcv;

	//public static RawId		GetId(string t) => new RawId(Encoding.UTF8.GetBytes(t.ToLower(), 0, 3).Order().ToArray());
	public static RawId		GetId(int n, string t, int start) => new RawId(Encoding.UTF8.GetBytes(t.ToLower(), start, n).Order().ToArray());

	public Ngram()
	{
	}

	public Ngram(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public Ngram Clone()
	{
		var a = new Ngram(Mcv)  {Id			= Id,
								 Ngrams		= Ngrams,
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
		Ngrams		= reader.ReadArray<RawId>();
		References	= reader.ReadArray<TextReference>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Ngrams);
		writer.Write(References);
	}
}
