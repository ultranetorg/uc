using System.Text;

namespace Uccs.Fair;

public enum EntityTextField : byte
{
	AccountNickname, 
	AuthorNickname, 
	SiteNickname, 
//
//	AuthorTitle,
//	SiteTitle,
//	PublicationTitle,
}

public class EntityFieldAddress : IBinarySerializable, IComparable<EntityFieldAddress>
{
	public AutoId				Entity { get; set; }
	public EntityTextField		Field { get; set; }

	public EntityFieldAddress()
	{
	}

	public EntityFieldAddress(AutoId entity, EntityTextField field)
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
		return $"{Entity}/{Field}";
	}

	public static EntityFieldAddress Parse(string t)
	{
		var e = new EntityFieldAddress();
		
		var i = t.IndexOf('/');
		
		e.Entity = AutoId.Parse(t.AsSpan(0, i));
		e.Field = Enum.Parse<EntityTextField>(t.AsSpan(i + 1));

		return e;
	}

	public void Read(BinaryReader reader)
	{
		Field	= reader.Read<EntityTextField>();
		Entity	= reader.Read<AutoId>();
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
	public EntityFieldAddress	Reference { get; set; }

	public EntityId				Key => Id;
	public bool					Deleted { get; set; }
	FairMcv						Mcv;

	public Word()
	{
	}

	public Word(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public override string ToString()
	{
		return $"{Id}, {Encoding.UTF8.GetString(Id.Bytes)}, Reference={Reference}";
	}

	public static RawId	GetId(string t)
	{
		var b = Encoding.UTF8.GetBytes(t);

		return new RawId(b);
	}

	public object Clone()
	{
		var a = new Word(Mcv)  {Id			= Id,
								Reference	= Reference};

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
		Reference	= reader.Read<EntityFieldAddress>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Reference);
	}
}
