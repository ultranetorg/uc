namespace Uccs.Net;

public struct OutwardResult : IBinarySerializable, IEquatable<OutwardResult>, IComparable<OutwardResult>
{
	public AutoId		User;
	public int			Id;
	public bool			Approved;

	public void Read(BinaryReader reader)
	{
		User		= reader.Read<AutoId>();
		Id			= reader.Read7BitEncodedInt();
		Approved	= reader.ReadBoolean();	
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(User);
		writer.Write7BitEncodedInt(Id);
		writer.Write(Approved);
	}

	public override bool Equals(object obj)
	{
		return obj is OutwardResult id && Equals(id);
	}

	public bool Equals(OutwardResult a)
	{
		return User == a.User && Id == a.Id && Approved == a.Approved;
	}

	public int CompareTo(OutwardResult a)
	{
		var c = User.CompareTo(a.User);

		if(c != 0)
			return c;

		c = Id.CompareTo(a.Id);

		if(c != 0)
			return c;

		c = Approved.CompareTo(a.Approved);

		if(c != 0)
			return c;

		return 0;
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}

	public static bool operator == (OutwardResult left, OutwardResult right)
	{
		return left.Equals(right);
	}

	public static bool operator != (OutwardResult left, OutwardResult right)
	{
		return !left.Equals(right);
	}

}
