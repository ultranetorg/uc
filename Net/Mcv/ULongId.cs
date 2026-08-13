namespace Uccs.Net;

public class ULongId : EntityId
{
	public ulong		Long { get; set; }
	public override int B  => (int)(Long);

	public ULongId()
	{
	}

	public ULongId(ulong b)
	{
		Long = b;
	}

	public override string ToString()
	{
		return $"{Long}";
	}

	public override int GetHashCode()
	{
		return B;
	}

	public override void Read(Reader reader)
	{
		Long = (ulong)reader.Read7BitEncodedInt64();
	}

	public override void Write(Writer writer)
	{
		writer.Write7BitEncodedInt64((long)Long);
	}

	public override bool Equals(object obj)
	{
		return obj is ULongId id && Equals(id);
	}

	public override bool Equals(EntityId a)
	{
		return a is ULongId e && Long == e.Long;
	}

	public override int CompareTo(EntityId a)
	{
		return CompareTo((ULongId)a);
	}

	public int CompareTo(ULongId a)
	{
		return Long.CompareTo(a.Long);
	}

	public static bool operator == (ULongId left, ULongId right)
	{
		return left is null && right is null || left is not null && left.Equals((object)right); /// object cast is IMPORTANT!!
	}

	public static bool operator != (ULongId left, ULongId right)
	{
		return !(left == right);
	}
}
