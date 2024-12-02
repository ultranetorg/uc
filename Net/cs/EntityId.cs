namespace Uccs.Net
{
	public abstract class BaseId : IBinarySerializable, IEquatable<EntityId>, IComparable<EntityId>
	{
		public ushort			C { get; set; }

		public abstract int		CompareTo(EntityId other);
		public abstract bool	Equals(EntityId other);
		public abstract void	Read(BinaryReader reader);
		public abstract void	Write(BinaryWriter writer);
	}

	public class EntityId : BaseId
	{
		public int		E { get; set; }

		public EntityId()
		{
		}

		public EntityId(ushort ci, int ei)
		{
			C = ci;
			E = ei;
		}

		public override string ToString()
		{
			return $"{C}-{E}";
		}

		public static EntityId Parse(string t)
		{
			var i = t.IndexOf('-');

			return new EntityId(ushort.Parse(t.Substring(0, i)), int.Parse(t.Substring(i + 1)));
		}

		public override void Read(BinaryReader reader)
		{
			C	= reader.ReadUInt16();
			E	= reader.Read7BitEncodedInt();
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write(C);
			writer.Write7BitEncodedInt(E);
		}

		public override bool Equals(object obj)
		{
			return obj is EntityId id && Equals(id);
		}

		public override bool Equals(EntityId a)
		{
			return a is not null && C == a.C && E == a.E;
		}

		public override int CompareTo(EntityId a)
		{
			if(C != a.C)
				return C.CompareTo(a.C);
			
			if(E != a.E)
				return E.CompareTo(a.E);

			return 0;
		}

		public override int GetHashCode()
		{
			return C.GetHashCode();
		}

		public static bool operator == (EntityId left, EntityId right)
		{
			return left is null && right is null || left is not null && left.Equals((object)right); /// object cast is IMPORTANT!!
		}

		public static bool operator != (EntityId left, EntityId right)
		{
			return !(left == right);
		}
	}
}
