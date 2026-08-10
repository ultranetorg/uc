namespace Uccs.Net;

public class EntityFieldAddress<E> : IBinarySerializable, IComparable<EntityFieldAddress<E>> where E : unmanaged, Enum
{
	public AutoId	Id { get; set; }
	public E		Field { get; set; }

	public EntityFieldAddress()
	{
	}

	public EntityFieldAddress(AutoId entity, E field)
	{
		Id = entity;
		Field = field;
	}

	public EntityFieldAddress(byte[] raw)
	{
		using var r = new Reader(raw);
		Read(r);
	}

	public override string ToString()
	{
		return $"{Id}/{Field}";
	}

	public static EntityFieldAddress<E> Parse(string t)
	{
		var e = new EntityFieldAddress<E>();
		
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

	public int CompareTo(EntityFieldAddress<E> x)
	{
		var c = Id.CompareTo(Id);

		return c != 0 ? c : Field.CompareTo(x.Field);
	}
}
