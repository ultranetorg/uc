using System.Numerics;

namespace Uccs.Net;

public struct ForeignResult : IBinarySerializable, IEquatable<ForeignResult>, IComparable<ForeignResult>
{
	public OperationId	Id;
	public bool			Approved;

	public void Read(BinaryReader reader)
	{
		Id			= reader.Read<OperationId>();
		Approved	= reader.ReadBoolean();	
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Approved);
	}

	public override bool Equals(object obj)
	{
		return obj is ForeignResult id && Equals(id);
	}

	public bool Equals(ForeignResult a)
	{
		return Id == a.Id && Approved == a.Approved;
	}

	public int CompareTo(ForeignResult a)
	{
		var c = Id.CompareTo(a.Id);

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

	public static bool operator == (ForeignResult left, ForeignResult right)
	{
		return left.Equals(right);
	}

	public static bool operator != (ForeignResult left, ForeignResult right)
	{
		return !left.Equals(right);
	}

}

public struct OperationId : IBinarySerializable, IEquatable<OperationId>, IComparable<OperationId>
{
	public int	Ri { get; private set; } = -1;
	public int	Ti { get; private set; }
	public int	Oi { get; private set; }

	byte[]			_Serial;

	public OperationId(int ri, int ti, byte oi)
	{
		Ri = ri;
		Ti = ti;
		Oi = oi;
	}	

	public override string ToString()
	{
		return $"{Ri}-{Ti}-{Oi}";
	}

	public BigInteger AsBigInteger => new BigInteger(AsBytes, true, true);
	
	byte[] AsBytes
	{
		get
		{
			if(_Serial == null)
			{
				var s = new MemoryStream();
				var w = new BinaryWriter(s);
				Write(w);

				_Serial = s.ToArray();
			}

			return _Serial;
		}
	}


	public void Read(BinaryReader reader)
	{
		Ri	= reader.Read7BitEncodedInt();
		Ti	= reader.Read7BitEncodedInt();
		Oi	= reader.Read7BitEncodedInt();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write7BitEncodedInt(Ri);
		writer.Write7BitEncodedInt(Ti);
		writer.Write7BitEncodedInt(Oi);
	}

	public override bool Equals(object obj)
	{
		return obj is OperationId id && Equals(id);
	}

	public bool Equals(OperationId a)
	{
		return Ri == a.Ri && Ti == a.Ti && Oi == a.Oi;
	}

	public int CompareTo(OperationId a)
	{
		var c =	Ri.CompareTo(a.Ri);
		if(c !=0)
			return c;

		c = Ti.CompareTo(a.Ti);
		if(c !=0)
			return c;

		c = Oi.CompareTo(a.Oi);
		if(c !=0)
			return c;

		return 0;
	}

	public override int GetHashCode()
	{
		return Ri.GetHashCode();
	}

	public static bool operator == (OperationId left, OperationId right)
	{
		return left.Equals(right);
	}

	public static bool operator != (OperationId left, OperationId right)
	{
		return !(left == right);
	}
}
