namespace Uccs.Net
{
	public class EntityId : IBinarySerializable, IEquatable<EntityId>, IComparable<EntityId>
	{
		public byte[]	Ci { get; set; }
		public int		Ei { get; set; }

		public EntityId()
		{
		}

		public EntityId(byte[] ci, int ei)
		{
			Ci = ci;
			Ei = ei;
		}

		public override string ToString()
		{
			return $"{Ci?.ToHex()}-{Ei}";
		}

		public static EntityId Parse(string t)
		{
			var i = t.IndexOf('-');

			return new EntityId(t.Substring(0, i).FromHex(), int.Parse(t.Substring(i + 1)));
		}

		public void Read(BinaryReader reader)
		{
			Ci	= reader.ReadBytes(TableBase.ClusterBase.IdLength);
			Ei	= reader.Read7BitEncodedInt();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(Ci);
			writer.Write7BitEncodedInt(Ei);
		}

		public override bool Equals(object obj)
		{
			return obj is EntityId id && Equals(id);
		}

		public bool Equals(EntityId a)
		{
			return a is not null && Ci[0] == a.Ci[0] && Ci[1] == a.Ci[1] && Ei == a.Ei;
		}

		public int CompareTo(EntityId a)
		{
			if(!Ci.SequenceEqual(a.Ci))	
				return Bytes.Comparer.Compare(Ci, a.Ci);
			
			if(Ei != a.Ei)
				return Ei.CompareTo(a.Ei);

			return 0;
		}

		public override int GetHashCode()
		{
			return Ci.GetHashCode();
		}

		public static bool operator == (EntityId left, EntityId right)
		{
			return left is null && right is null || left is not null && left.Equals(right);
		}

		public static bool operator != (EntityId left, EntityId right)
		{
			return !(left == right);
		}
	}
}
