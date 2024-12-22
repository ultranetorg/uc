using System.Numerics;

namespace Uccs.Net;

public struct ForeignResult : IBinarySerializable, IEquatable<ForeignResult>, IComparable<ForeignResult>
{
	public OperationId	OperationId;
	public bool			Approved;

	public void Read(BinaryReader reader)
	{
		OperationId = reader.Read<OperationId>();
		Approved	= reader.ReadBoolean();	
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(OperationId);
		writer.Write(Approved);
	}

	public override bool Equals(object obj)
	{
		return obj is ForeignResult id && Equals(id);
	}

	public bool Equals(ForeignResult a)
	{
		return OperationId == a.OperationId && Approved == a.Approved;
	}

	public int CompareTo(ForeignResult a)
	{
		if(OperationId != a.OperationId)	return OperationId.CompareTo(a.OperationId);
		if(Approved != a.Approved)			return Approved.CompareTo(a.Approved);

		return 0;
	}

	public override int GetHashCode()
	{
		return OperationId.GetHashCode();
	}

	public static bool operator == (ForeignResult left, ForeignResult right)
	{
		return left.Equals(right);
	}

	public static bool operator != (ForeignResult left, ForeignResult right)
	{
		return !(left == right);
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
		if(Ri != a.Ri)	return Ri.CompareTo(a.Ri);
		if(Ti != a.Ti)	return Ti.CompareTo(a.Ti);
		if(Oi != a.Oi)	return Oi.CompareTo(a.Oi);

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
