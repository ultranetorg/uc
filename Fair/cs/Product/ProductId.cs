namespace Uccs.Fair
{
	public class ProductId : IBinarySerializable, IEquatable<ProductId>, IComparable<ProductId>
	{
		public ushort	C { get; set; }
		public int		D { get; set; }
		public int		P { get; set; }

		public EntityId	PublisherId => new EntityId(C, D);

		public ProductId()
		{
		}

		public ProductId(ushort c, int d, int p)
		{
			C = c;
			D = d;
			P = p;
		}

		public override string ToString()
		{
			return $"{C}-{D}-{P}";
		}

		public static ProductId Parse(string t)
		{
			var a = t.Split('-');

			return new ProductId(ushort.Parse(a[0]), int.Parse(a[1]), int.Parse(a[2]));
		}

		public void Read(BinaryReader reader)
		{
			C	= reader.ReadUInt16();
			D	= reader.Read7BitEncodedInt();
			P	= reader.Read7BitEncodedInt();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(C);
			writer.Write7BitEncodedInt(D);
			writer.Write7BitEncodedInt(P);
		}

		public override bool Equals(object obj)
		{
			return obj is ProductId id && Equals(id);
		}

		public bool Equals(ProductId a)
		{
			return a is not null && C == a.C && D == a.D && P == a.P;
		}

		public int CompareTo(ProductId a)
		{
			if(C != a.C)
				return C.CompareTo(a.C);
			
			if(D != a.D)
				return D.CompareTo(a.D);

			if(P != a.P)
				return P.CompareTo(a.P);

			return 0;
		}

		public override int GetHashCode()
		{
			return C.GetHashCode();
		}

		public static bool operator == (ProductId left, ProductId right)
		{
			return left is null && right is null || left is not null && left.Equals(right);
		}

		public static bool operator != (ProductId left, ProductId right)
		{
			return !(left == right);
		}
	}
}
