namespace Uccs.Fair;

public class ProductId : EntityId, IEquatable<ProductId>, IComparable<ProductId>
{
	public int			P { get; set; }
	public EntityId		AuthorId => new EntityId(H, E);

	public ProductId()
	{
	}

	public ProductId(int c, int d, int r)
	{
		H = c;
		E = d;
		P = r;
	}

	public override string ToString()
	{
		return $"{H}-{E}-{P}";
	}

	public new static ProductId Parse(string t)
	{
		var a = t.Split('-');

		return new ProductId(int.Parse(a[0]), int.Parse(a[1]), int.Parse(a[2]));
	}

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);
		P = reader.Read7BitEncodedInt();
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);
		writer.Write7BitEncodedInt(P);
	}

	public override bool Equals(object obj)
	{
		return obj is ProductId id && Equals(id);
	}

	public bool Equals(ProductId a)
	{
		return a is not null && H == a.H && E == a.E && P == a.P;
	}

	public override int CompareTo(BaseId a)
	{
		return CompareTo((ProductId)a);
	}

	public int CompareTo(ProductId a)
	{
		if(H != a.H)	
			return H.CompareTo(a.H);
		
		if(E != a.E)
			return E.CompareTo(a.E);

		if(P != a.P)
			return P.CompareTo(a.P);

		return 0;
	}

	public override int GetHashCode()
	{
		return H.GetHashCode();
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
