using System;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public struct ResourceId : IBinarySerializable, IEquatable<ResourceId>, IComparable<ResourceId>
	{
		public byte[]	Ci { get; set; }
		public int		Ai { get; set; }
		public int		Ri { get; set; }
		byte[]			_Serial;

		public ResourceId(byte[] ci, int ai, int ri)
		{
			Ci = ci;
			Ai = ai;
			Ri = ri;
		}

		public byte[] Serial
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

		public override string ToString()
		{
			return $"{Ci?.ToHex()}-{Ai}-{Ri}";
		}

		public void Read(BinaryReader reader)
		{
			Ci	= reader.ReadBytes(AuthorTable.Cluster.IdLength);
			Ai	= reader.Read7BitEncodedInt();
			Ri	= reader.Read7BitEncodedInt();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(Ci);
			writer.Write7BitEncodedInt(Ai);
			writer.Write7BitEncodedInt(Ri);
		}

		public override bool Equals(object obj)
		{
			return obj is ResourceId id && Equals(id);
		}

		public bool Equals(ResourceId a)
		{
			return Ci == a.Ci && Ai == a.Ai && Ri == a.Ri;
		}

		public int CompareTo(ResourceId a)
		{
			if(!Ci.SequenceEqual(a.Ci))	
				return Bytes.Comparer.Compare(Ci, a.Ci);
			
			if(Ai != a.Ai)
				return Ai.CompareTo(a.Ai);

			if(Ri != a.Ri)
				return Ri.CompareTo(a.Ri);

			return 0;
		}

		public override int GetHashCode()
		{
			return Ci.GetHashCode();
		}

		public static bool operator == (ResourceId left, ResourceId right)
		{
			return left.Equals(right);
		}

		public static bool operator != (ResourceId left, ResourceId right)
		{
			return !(left == right);
		}
	}
}
