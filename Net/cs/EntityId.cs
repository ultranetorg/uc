namespace Uccs.Net
{
	public class EntityId : IBinarySerializable, IEquatable<EntityId>, IComparable<EntityId>
	{
		public ushort	Ci { get; set; }
		public int		Ei { get; set; }

		public EntityId()
		{
		}

		public EntityId(ushort ci, int ei)
		{
			Ci = ci;
			Ei = ei;
		}

		public override string ToString()
		{
			return $"{Ci}-{Ei}";
		}

		public static EntityId Parse(string t)
		{
			var i = t.IndexOf('-');

			return new EntityId(ushort.Parse(t.Substring(0, i)), int.Parse(t.Substring(i + 1)));
		}

		public void Read(BinaryReader reader)
		{
			Ci	= reader.ReadUInt16();
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
			return a is not null && Ci == a.Ci && Ei == a.Ei;
		}

		public int CompareTo(EntityId a)
		{
			if(Ci != a.Ci)
				return Ci.CompareTo(a.Ci);
			
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
