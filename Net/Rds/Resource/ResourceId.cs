using System;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class ResourceId : IBinarySerializable, IEquatable<ResourceId>, IComparable<ResourceId>
	{
		public byte[]	Ci { get; set; }
		public int		Di { get; set; }
		public int		Ri { get; set; }
		byte[]			_Serial;

		public EntityId	DomainId => new EntityId(Ci, Di);

		public ResourceId()
		{
		}

		public ResourceId(byte[] ci, int ai, int ri)
		{
			Ci = ci;
			Di = ai;
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
			return $"{Ci?.ToHex()}-{Di}-{Ri}";
		}

		public static ResourceId Parse(string t)
		{
			var a = t.Split('-');

			return new ResourceId(a[0].FromHex(), int.Parse(a[1]), int.Parse(a[2]));
		}

		public void Read(BinaryReader reader)
		{
			Ci	= reader.ReadBytes(DomainTable.Cluster.IdLength);
			Di	= reader.Read7BitEncodedInt();
			Ri	= reader.Read7BitEncodedInt();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(Ci);
			writer.Write7BitEncodedInt(Di);
			writer.Write7BitEncodedInt(Ri);
		}

		public override bool Equals(object obj)
		{
			return obj is ResourceId id && Equals(id);
		}

		public bool Equals(ResourceId a)
		{
			return a is not null && Ci[0] == a.Ci[0] && Ci[1] == a.Ci[1] && Di == a.Di && Ri == a.Ri;
		}

		public int CompareTo(ResourceId a)
		{
			if(!Ci.SequenceEqual(a.Ci))	
				return Bytes.Comparer.Compare(Ci, a.Ci);
			
			if(Di != a.Di)
				return Di.CompareTo(a.Di);

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
			return left is null && right is null || left is not null && left.Equals(right);
		}

		public static bool operator != (ResourceId left, ResourceId right)
		{
			return !(left == right);
		}
	}
}
