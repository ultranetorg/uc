namespace Uccs.Rdn;

public class ResourceId : EntityId, IEquatable<ResourceId>, IComparable<ResourceId>
{
	public int		R { get; set; }

	public ResourceId()
	{
	}

	public ResourceId(int c, int d, int r)
	{
		B = c;
		E = d;
		R = r;
	}

	public override string ToString()
	{
		return $"{B}-{E}-{R}";
	}

	public new static ResourceId Parse(string t)
	{
		var a = t.Split('-');

		return new ResourceId(int.Parse(a[0]), int.Parse(a[1]), int.Parse(a[2]));
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
		return a is not null && B == a.B && E == a.E && R == a.R;
	}

	public override int CompareTo(BaseId a)
	{
		return CompareTo((ResourceId)a);
	}

	public int CompareTo(ResourceId a)
	{
		if(B != a.B)	
			return B.CompareTo(a.B);
		
		if(E != a.E)
			return E.CompareTo(a.E);

		if(R != a.R)
			return R.CompareTo(a.R);

		return 0;
	}

	public override int GetHashCode()
	{
		return B.GetHashCode();
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
