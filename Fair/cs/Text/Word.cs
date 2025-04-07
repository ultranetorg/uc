using System.Text;

namespace Uccs.Fair;

public enum EntityTextField : byte
{
	AccountNickname, 

	AuthorNickname, 
	AuthorTitle,

	SiteNickname, 
	SiteTitle,

	PublicationTitle,
}

public class EntityFieldAddress : IBinarySerializable, IComparable<EntityFieldAddress>
{
	public EntityId				Entity { get; set; }
	public EntityTextField		Field { get; set; }

	public EntityFieldAddress()
	{
	}

	public EntityFieldAddress(EntityId entity, EntityTextField field)
	{
		Entity = entity;
		Field = field;
	}

	public EntityFieldAddress(byte[] raw)
	{
		Read(new BinaryReader(new MemoryStream(raw)));
	}

	public override string ToString()
	{
		return $"{Entity}/{(byte)Field}";
	}

	public static EntityFieldAddress Parse(string t)
	{
		var e = new EntityFieldAddress();
		
		var i = t.IndexOf('/');
		
		e.Entity = EntityId.Parse(t.Substring(0, i));
		e.Field = (EntityTextField)byte.Parse(t.Substring(i + 1));

		return e;
	}

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

	public int CompareTo(EntityFieldAddress x)
	{
		var c = Entity.CompareTo(Entity);

		return c != 0 ? c : Field.CompareTo(x.Field);
	}
}

public class Word : IBinarySerializable, ITableEntry
{
	public RawId				Id { get; set; }
	public EntityFieldAddress[]	References { get; set; }

	public BaseId				Key => Id;
	public bool					Deleted { get; set; }
	FairMcv						Mcv;


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
		References	= reader.ReadArray<EntityFieldAddress>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(References);
	}
}
