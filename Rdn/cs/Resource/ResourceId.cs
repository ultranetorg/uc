namespace Uccs.Rdn
{
	public class ResourceId : EntityId, IEquatable<ResourceId>, IComparable<ResourceId>
	{
		public int		R { get; set; }

		public ResourceId()
		{
		}

		public ResourceId(ushort c, int d, int r)
		{
			C = c;
			E = d;
			R = r;
		}

		public override string ToString()
		{
			return $"{C}-{E}-{R}";
		}

		public new static ResourceId Parse(string t)
		{
			var a = t.Split('-');

			return new ResourceId(ushort.Parse(a[0]), int.Parse(a[1]), int.Parse(a[2]));
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			R = reader.Read7BitEncodedInt();
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write7BitEncodedInt(R);
		}

		public override bool Equals(object obj)
		{
			return obj is ResourceId id && Equals(id);
		}

		public bool Equals(ResourceId a)
		{
			return a is not null && C == a.C && E == a.E && R == a.R;
		}

		public int CompareTo(ResourceId a)
		{
			if(C != a.C)	
				return C.CompareTo(a.C);
			
			if(E != a.E)
				return E.CompareTo(a.E);

			if(R != a.R)
				return R.CompareTo(a.R);

			return 0;
		}

		public override int GetHashCode()
		{
			return C.GetHashCode();
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
