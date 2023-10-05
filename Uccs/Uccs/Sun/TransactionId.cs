using System;
using System.IO;

namespace Uccs.Net
{
	public struct TransactionId : IBinarySerializable, IEquatable<TransactionId>, IComparable<TransactionId>
	{
		public const int	MaxRi = 0b1111111_1111111_1111111_1111111;
		public const int	MaxTi = 0b1111111_1111111_1111111;

		public int			Ri { get; private set; }
		public int			Ti { get; private set; }
		long				_Number = -1;

		public TransactionId(int ri, int ti)
		{
			if(ri > MaxRi)	throw new NotSupportedException();
			if(ti > MaxTi)	throw new NotSupportedException();

			Ri = ri;
			Ti = ti;
		}

		public long Number
		{
			get
			{
				if(_Number == -1)
				{
					var s = new MemoryStream(8);
					var w = new BinaryWriter(s);
					Write(w);
	
					_Number = BitConverter.ToInt64(s.GetBuffer());
				}

				return _Number;
			}
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
			return Ri;
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
