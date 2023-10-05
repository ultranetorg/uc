using System;
using System.IO;

namespace Uccs.Net
{
	public struct OperationId : IBinarySerializable, IEquatable<OperationId>, IComparable<OperationId>
	{
		public const byte MaxOi = 0b1111111;

		public int		Ri { get; private set; } = -1;
		public int		Ti { get; private set; }
		public byte		Oi { get; private set; }

		long			_Number = -1;

		public OperationId(int ri, int ti, byte oi)
		{
			if(ri > TransactionId.MaxRi)	throw new NotSupportedException();
			if(ti > TransactionId.MaxTi)	throw new NotSupportedException();
			if(oi > MaxOi)					throw new NotSupportedException();

			Ri = ri;
			Ti = ti;
			Oi = oi;
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
			Oi	= reader.ReadByte();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt(Ri);
			writer.Write7BitEncodedInt(Ti);
			writer.Write(Oi);
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
			return Ri;
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
}
