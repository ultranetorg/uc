namespace Uccs.Net;

public enum MetaEntityType : int
{
	None,
	AccountCount,
	_Last = 1000
}

public class MetaId : EntityId
{
	public int			Type;
	public byte[]		Index;
	
	public override int B
	{
		get => Type; 
		set => throw new NotSupportedException();
	}

	public MetaId()
	{
	}

	public MetaId(int type, byte[] index)
	{
		Type = type;
		Index = index;
	}

	public MetaId(int type)
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
		return Type;
	}

	public override void Read(BinaryReader reader)
	{
		Type = reader.Read7BitEncodedInt();
		Index = reader.ReadBytes() ?? [];
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write7BitEncodedInt(Type);
		writer.WriteBytes(Index);
	}

	public override bool Equals(object obj)
	{
		return obj is MetaId id && Equals(id);
	}

	public override bool Equals(EntityId a)
	{
		return a is MetaId e && Type == e.Type && Index.SequenceEqual(e.Index);
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

public class MetaEntity : IBinarySerializable, ITableEntry
{
	public MetaId		Id { get; set; }
	public byte[]		Value { get; set; }

	public EntityId		Key => Id;
	public bool			Deleted { get; set; }

	Mcv					Mcv;

	public override string ToString()
	{
		return $"{Id}, {Value}";
	}

	public virtual void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.WriteBytes(Value);
	}

	public virtual void Read(BinaryReader reader)
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

	public virtual void WriteMain(BinaryWriter w)
	{
		Write(w);
	}

	public virtual void ReadMain(BinaryReader r)
	{
		Read(r);
	}

	public void Cleanup(Round lastInCommit)
	{
	}
}

public class MetaTable : Table<MetaId, MetaEntity>
{
	public MetaTable(Mcv chain) : base(chain)
	{
	}

	public override MetaEntity Create()
	{
		return new MetaEntity(Mcv);
	}
}
