using System;
using System.IO;

namespace Uccs.Net
{
	public struct TransactionId : IBinarySerializable, IEquatable<TransactionId>, IComparable<TransactionId>
	{
		public int	Ri { get; private set; }
		public int	Ti { get; private set; }

		public TransactionId()
		{
		}

		public TransactionId(int ri, int ti)
		{
			Ri = ri;
			Ti = ti;
		}

		public override string ToString()
		{
			if(Ti == -1)
				Ti = Ti;

			return $"{Ri}-{Ti}";
		}

		public void Read(BinaryReader reader)
		{
			Ri	= reader.Read7BitEncodedInt();
			Ti	= reader.Read7BitEncodedInt();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt(Ri);
			writer.Write7BitEncodedInt(Ti);
		}

		public override bool Equals(object obj)
		{
			return obj is TransactionId id && Equals(id);
		}

		public bool Equals(TransactionId a)
		{
			return Ri == a.Ri && Ti == a.Ti;
		}

		public int CompareTo(TransactionId a)
		{
			if(Ri != a.Ri)	return Ri.CompareTo(a.Ri);
			if(Ti != a.Ti)	return Ti.CompareTo(a.Ti);

			return 0;
		}

		public override int GetHashCode()
		{
			return Ri.GetHashCode();
		}

		public static bool operator == (TransactionId left, TransactionId right)
		{
			return left.Equals(right);
		}

		public static bool operator != (TransactionId left, TransactionId right)
		{
			return !(left == right);
		}
	}
}
	