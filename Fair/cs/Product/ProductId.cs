namespace Uccs.Fair
{
	public class ProductId : IBinarySerializable, IEquatable<ProductId>, IComparable<ProductId>
	{
		public byte[]	C { get; set; }
		public int		D { get; set; }
		public int		P { get; set; }
		byte[]			_Serial;

		public EntityId	PublisherId => new EntityId(C, D);

		public ProductId()
		{
		}

		public ProductId(byte[] c, int d, int p)
		{
			C = c;
			D = d;
			P = p;
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
			return $"{C?.ToHex()}-{D}-{P}";
		}

		public static ProductId Parse(string t)
		{
			var a = t.Split('-');

			return new ProductId(a[0].FromHex(), int.Parse(a[1]), int.Parse(a[2]));
		}

		public void Read(BinaryReader reader)
		{
			C	= reader.ReadBytes(PublisherTable.Cluster.IdLength);
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
			return a is not null && C[0] == a.C[0] && C[1] == a.C[1] && D == a.D && P == a.P;
		}

		public int CompareTo(ProductId a)
		{
			if(!C.SequenceEqual(a.C))	
				return Bytes.Comparer.Compare(C, a.C);
			
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
