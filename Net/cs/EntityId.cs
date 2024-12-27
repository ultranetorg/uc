namespace Uccs.Net;

public abstract class BaseId : IBinarySerializable, IEquatable<BaseId>, IComparable<BaseId>
{
	public int				B { get; set; }

	public abstract int		CompareTo(BaseId other);
	public abstract bool	Equals(BaseId other);
	public abstract void	Read(BinaryReader reader);
	public abstract void	Write(BinaryWriter writer);

 	public static bool operator == (BaseId left, BaseId right)
 	{
 		return left is null && right is null || left is not null && left.Equals((object)right); /// object cast is IMPORTANT!!
 	}
 
 	public static bool operator != (BaseId left, BaseId right)
 	{
 		return !(left == right);
 	}

	public override int GetHashCode()
	{
		return B.GetHashCode();
	}

	public override abstract bool Equals(object obj);
}

public class EntityId : BaseId
{
	public int		E { get; set; }

	public EntityId()
	{
	}

	public EntityId(int ci, int ei)
	{
		B = ci;
		E = ei;
	}

	public override string ToString()
	{
		return $"{B}-{E}";
	}

	public override int GetHashCode()
	{
		return B.GetHashCode();
	}

	public static EntityId Parse(string t)
	{
		var i = t.IndexOf('-');

		return new EntityId(int.Parse(t.Substring(0, i)), int.Parse(t.Substring(i + 1)));
	}

	public override void Read(BinaryReader reader)
	{
		B	= reader.Read7BitEncodedInt();
		E	= reader.Read7BitEncodedInt();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write7BitEncodedInt(B);
		writer.Write7BitEncodedInt(E);
	}

	public override bool Equals(object obj)
	{
		return obj is EntityId id && Equals(id);
	}

	public override bool Equals(BaseId a)
	{
		return a is EntityId e && B == a.B && E == e.E;
	}

	public override int CompareTo(BaseId a)
	{
		return CompareTo((EntityId)a);
	}

	public int CompareTo(EntityId a)
	{
		if(B != a.B)
			return B.CompareTo(a.B);
		
		if(E != a.E)
			return E.CompareTo(a.E);

		return 0;
	}

	public static bool operator == (EntityId left, EntityId right)
	{
		return left is null && right is null || left is not null && left.Equals((object)right); /// object cast is IMPORTANT!!
	}

	public static bool operator != (EntityId left, EntityId right)
	{
		return !(left == right);
	}
}
