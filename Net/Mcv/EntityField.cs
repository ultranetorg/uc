namespace Uccs.Net;

public class EntityField<E> : IBinarySerializable, IComparable<EntityField<E>> where E : unmanaged, Enum
{
	public AutoId	Id { get; set; }
	public E		Field { get; set; }

	public EntityField()
	{
	}

	public EntityField(AutoId entity, E field)
	{
		Id = entity;
		Field = field;
	}

	public EntityField(byte[] raw)
	{
		using var r = new Reader(raw);
		Read(r);
	}

	public override string ToString()
	{
		return $"{Id}/{Field}";
	}

	public static EntityField<E> Parse(string t)
	{
		var e = new EntityField<E>();
		
		var i = t.IndexOf('/');
		
		e.Id = AutoId.Parse(t.AsSpan(0, i));
		e.Field = (E)Enum.Parse(typeof(E), t.AsSpan(i + 1));

		return e;
	}

	public void Read(Reader reader)
	{
		Field	= reader.Read<E>();
		Id	= reader.Read<AutoId>();
	}

	public void Write(Writer writer)
	{
		writer.Write(Field);
		writer.Write(Id);
	}

	public int CompareTo(EntityField<E> x)
	{
		var c = Id.CompareTo(Id);

		return c != 0 ? c : Field.CompareTo(x.Field);
	}
}
