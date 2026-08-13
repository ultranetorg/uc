namespace Uccs.Net;

public enum MetaEntityType : uint
{
	None,
	UserIdCounter	= 000_000_001,
	FriendIdCounter	= 000_000_002,
	UserCount		= 001_000_001,
	_Last			= 009_999_999
}

public class MetaId : EntityId
{
	public uint			Type;
	public byte[]		Index; /// optional
	
	public override int B
	{
		get => (int)Type; 
	}

	public MetaId()
	{
	}

	public MetaId(uint type, byte[] index)
	{
		Type = type;
		Index = index;
	}

	public MetaId(uint type)
	{
		Type = type;
		Index = [];
	}

	public override string ToString()
	{
		return Type.ToString();
	}

	public override int GetHashCode()
	{
		return (int)Type;
	}

	public override void Read(Reader reader)
	{
		Type = (uint)reader.Read7BitEncodedInt();
		Index = reader.ReadBytes() ?? [];
	}

	public override void Write(Writer writer)
	{
		writer.Write7BitEncodedInt((int)Type);
		writer.WriteBytes(Index);
	}

	public override bool Equals(object obj)
	{
		return obj is MetaId id && Equals(id);
	}

	public override bool Equals(EntityId a)
	{
		return a is MetaId e && Type == e.Type && Bytes.EqualityComparer.Equals(Index, e.Index);
	}

	public override int CompareTo(EntityId a)
	{
		return CompareTo((MetaId)a);
	}

	public int CompareTo(MetaId a)
	{
		var c = Type.CompareTo(a.Type);

		if(c != 0)
			return c;

		return Bytes.Comparer.Compare(Index, a.Index);
	}

	public static bool operator == (MetaId left, MetaId right)
	{
		return left is null && right is null || left is not null && left.Equals(right); /// object cast is IMPORTANT!!
	}

	public static bool operator != (MetaId left, MetaId right)
	{
		return !(left == right);
	}
}

public class MetaEntity : IBinarySerializable, ITableEntry<MetaId>
{
	public MetaId		Id { get; set; }
	public byte[]		Value { get; set; }

	public int			AsInt => BitConverter.ToInt32(Value);

	public bool			Deleted { get; set; }

	Mcv					Mcv;

	public override string ToString()
	{
		return $"{Id}, {Value}";
	}

	public virtual void Write(Writer writer)
	{
		writer.Write(Id);
		writer.WriteBytes(Value);
	}

	public virtual void Read(Reader reader)
	{
		Id		= reader.Read<MetaId>();
		Value	= reader.ReadBytes();
	}

	public MetaEntity()
	{
	}

	public MetaEntity(Mcv mcv)
	{
		Mcv = mcv;
	}

	public virtual object Clone()
	{
		var a = new MetaEntity();

		a.Id			= Id;
		a.Value			= Value;

		return a;
	}

	public virtual void WriteMain(Writer writer)
	{
		Write(writer);
	}

	public virtual void ReadMain(Reader reader)
	{
		Read(reader);
	}

	public void Cleanup(Round lastInCommit)
	{
	}
}

public class MetaTable : Table<MetaId, MetaEntity>
{
	public MetaTable(Mcv chain) : base(chain, McvTable.Meta.ToString())
	{
	}

	public override MetaEntity Create()
	{
		return new MetaEntity(Mcv);
	}
}
