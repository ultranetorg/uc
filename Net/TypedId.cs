namespace Uccs.Net;

public class TypedId<T> : AutoId where T : unmanaged, Enum
{
	public T Type { get; set; }

	public TypedId()
	{
	}

	public TypedId(int b, int e, T t) : base(b, e)
	{
		Type = t;
	}


	public override string ToString()
	{
		return $"{B}-{I}-{(byte)(object)Type}";
	}

	public override int GetHashCode()
	{
		return B;
	}

	public static bool TryParse(string text, out TypedId<T> entity)
	{
		var i = text.IndexOf('-');

		entity = null;

		if(i == -1)
			return false;

		var j = text.IndexOf('-', i + 1);

		if(j == -1)
			return false;

		if(int.TryParse(text.AsSpan(0, i), out var b) && int.TryParse(text.AsSpan(i+1, j-i-1), out var e) && byte.TryParse(text.AsSpan(j+1), out var t))
		{
			if(b < 0 || b >= TableBase.BucketsCountMax)
				return false;

			if(e < 0)
				return false;

			entity = new TypedId<T>(b, e, (T)Enum.ToObject(typeof(T), t));
			
			return true;
		}
		else
			return false;
	}

	public static new TypedId<T> Parse(string t)
	{
		if(TryParse(t, out var e))
			return e;
		else
			throw new FormatException();
	}

	public override void Read(BinaryReader reader)
	{
		B		= reader.Read7BitEncodedInt();
		I		= reader.Read7BitEncodedInt();
		Type	= reader.Read<T>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write7BitEncodedInt(B);
		writer.Write7BitEncodedInt(I);
		writer.Write(Type);
	}

	public override bool Equals(object obj)
	{
		return obj is TypedId<T> id && Equals(id);
	}

	public override bool Equals(EntityId a)
	{
		return a is TypedId<T> e && B == a.B && I == e.I && Type.Equals(e.Type);
	}

	public override int CompareTo(EntityId a)
	{
		return CompareTo((TypedId<T>)a);
	}

	public int CompareTo(TypedId<T> a)
	{
		var c = base.CompareTo(a);

		c = Type.CompareTo(a.Type);

		if(c != 0)
			return c;

		return 0;
	}

	public static bool operator == (TypedId<T> left, TypedId<T> right)
	{
		return left is null && right is null || left is not null && left.Equals((object)right); /// object cast is IMPORTANT!!
	}

	public static bool operator != (TypedId<T> left, TypedId<T> right)
	{
		return !(left == right);
	}
}
