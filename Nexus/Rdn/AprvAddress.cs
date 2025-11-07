namespace Uccs.Rdn;

/// <summary>
///  ultranet:netid:uo/app/dotnet/1.23.456
/// </summary>

public class AprvAddress : IBinarySerializable, IComparable, IComparable<AprvAddress>, IEquatable<AprvAddress>
{
	public string			Domain		{ get ; set; }
	public string			Product		{ get ; set; }
	public string			Realization { get ; set; }
	public string			Version		{ get ; set; }

	public string			DPR => $"{Domain}/{Product}/{Realization}";
	public string			PRV => $"{Product}/{Realization}/{Version}";

	public static implicit operator Ura(AprvAddress value) => new Ura(value.Domain, value.PRV);

	public AprvAddress(string domain, string product, string realization, string veriosn)
	{
		Domain = domain;
		Product = product;
		Realization = realization;
		Version = veriosn;
	}

	public AprvAddress(Ura resource, string version)
	{
		Domain = resource.Domain;

		var j = resource.Resource.LastIndexOf('/');
		
		Product		= resource.Resource.Substring(0, j);
		Realization = resource.Resource.Substring(j + 1);
		Version		= version; 
	}

	public AprvAddress(Ura resource)
	{
		Domain = resource.Domain;

		var j = resource.Resource.Split('/');
		
		Product		= j[0];
		Realization = j[1];
		Version		= j[2]; 
	}

	public AprvAddress()
	{
	}

	public override string ToString()
	{
		return new Ura(Domain, PRV).ToString();
	}

	public static AprvAddress Parse(string v)
	{
		var r = Ura.Parse(v);
		var a = new AprvAddress();
		var p = r.Resource.Split('/');

		a.Domain		= r.Domain;
		a.Product		= p[0];
		a.Realization	= p[1];
		a.Version		= p[2]; 

		return a;
	}

	public static bool TryParse(string v, out AprvAddress address)
	{
		var r = Ura.Parse(v);
		var p = r.Resource.Split('/');

		address = null;

		if(p.Length != 4)
			return false;

		address = new AprvAddress();

		address.Domain		= r.Domain;
		address.Product		= p[0];
		address.Realization	= p[1];
		address.Version		= p[2]; 

		return true;
	}

	public static bool TryParseAPR(string v, out AprvAddress address)
	{
		var r = Ura.Parse(v);
		var p = r.Resource.Split('/');

		address = null;

		if(p.Length != 4)
			return false;

		address = new AprvAddress();

		address.Domain		= r.Domain;
		address.Product		= p[0];
		address.Realization	= p[1];

		return true;
	}

	//public static Version ParseVesion(string v)
	//{
	//	return Version.Parse(v.Substring(v.LastIndexOf('/') + 1));
	//}

	public AprvAddress ReplaceVersion(string version)
	{
		return new AprvAddress(Domain, Product, Realization, version);
	}

	public int CompareTo(object obj)
	{
		return CompareTo(obj as AprvAddress);
	}

	public int CompareTo(AprvAddress o)
	{
		var a = Domain.CompareTo(o.Domain);
		if(a != 0)
			return a;

		a = Product.CompareTo(o.Product);
		if(a != 0)
			return a;

		a = Realization.CompareTo(o.Realization);
		if(a != 0)
			return a;

		if(Version is null && o.Version is not null)
			return -1;
		else if(Version is not null && o.Version is null)
			return 1;
		else
			return Version.CompareTo(o.Version);
	}
	
	public virtual void Write(BinaryWriter w)
	{
		w.WriteUtf8(Domain);
		w.WriteUtf8(Product);
		w.WriteUtf8(Realization);
		w.WriteUtf8(Version);
	}

	public virtual void Read(BinaryReader r)
	{
		Domain = r.ReadUtf8();
		Product = r.ReadUtf8();
		Realization = r.ReadUtf8();
		Version = r.ReadUtf8();
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as AprvAddress);
	}

	public bool Equals(AprvAddress o)
	{
		return	o is not null && 
				Domain		== o.Domain && 
				Product		== o.Product && 
				Realization	== o.Realization && 
				Version.Equals(o.Version);
	}

	public override int GetHashCode()
	{
		return Domain.GetHashCode();
	}

	public static bool operator == (AprvAddress left, AprvAddress right)
	{
		return left is null && right is null || left is not null && left.Equals(right);
	}

	public static bool operator != (AprvAddress left, AprvAddress right)
	{
		return !(left == right);
	}
}
