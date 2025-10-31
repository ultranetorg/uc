using Uccs.Net;
using Uccs.Rdn;

namespace Uccs.Nexus;

/// <summary>
///  scheme:net/uo/app/1.23.456
/// </summary>

public class ApvAddress : IBinarySerializable, IComparable, IComparable<ApvAddress>, IEquatable<ApvAddress>
{
	public string			Author		{ get ; set; }
	public string			Product		{ get ; set; }
	public string			Version		{ get ; set; }

	public string			AP => $"{Author}/{Product}";
	public string			PV => $"{Product}/{Version}";

	public static implicit operator Ura(ApvAddress value) => new Ura(value.Author, value.PV);

	public ApvAddress(string domain, string product, string veriosn)
	{
		Author = domain;
		Product = product;
		Version = veriosn;
	}

	public ApvAddress(Ura resource, string version)
	{
		Author = resource.Domain;

		var j = resource.Resource.IndexOf('/');
		
		Product		= resource.Resource.Substring(0, j);
		Version		= version; 
	}

	public ApvAddress(Ura resource)
	{
		Author = resource.Domain;

		var j = resource.Resource.LastIndexOf('/');
		
		Product		= resource.Resource.Substring(0, j);
		Version		= resource.Resource.Substring(j + 1); 
	}

	public ApvAddress()
	{
	}

	public override string ToString()
	{
		return new Ura(Author, PV).ToString();
	}

	public static ApvAddress Parse(string v)
	{
		TryParse(v, out var a);

		return a;
	}

	public static bool TryParse(string v, out ApvAddress address)
	{
		var u = Ura.Parse(v);

		address = new ApvAddress();
		
		address.Author = u.Domain;

		var j = u.Resource.LastIndexOf('/');

		if(j == -1)
			return false;
		
		address.Product		= u.Resource.Substring(0, j);
		address.Version		= u.Resource.Substring(j + 1); 

		return true;
	}

// 	public static bool TryParseAPR(string v, out ApvAddress address)
// 	{
// 		var r = Ura.Parse(v);
// 		var p = r.Resource.Split('/');
// 
// 		address = null;
// 
// 		if(p.Length != 4)
// 			return false;
// 
// 		address = new ApvAddress();
// 
// 		address.Author		= r.Domain;
// 		address.Product		= p[0];
// 		address.Version		= p[1];
// 
// 		return true;
// 	}

	//public static Version ParseVesion(string v)
	//{
	//	return Version.Parse(v.Substring(v.LastIndexOf('/') + 1));
	//}

	public ApvAddress ReplaceVersion(string version)
	{
		return new ApvAddress(Author, Product, version);
	}

	public int CompareTo(object obj)
	{
		return CompareTo(obj as ApvAddress);
	}

	public int CompareTo(ApvAddress o)
	{
		var a = Author.CompareTo(o.Author);
		if(a != 0)
			return a;

		a = Product.CompareTo(o.Product);
		if(a != 0)
			return a;

// 		if(Version is null && o.Version is not null)
// 			return -1;
// 		else if(Version is not null && o.Version is null)
// 			return 1;
// 		else
			return Version.CompareTo(o.Version);
	}
	
	public virtual void Write(BinaryWriter w)
	{
		w.WriteUtf8(Author);
		w.WriteUtf8(Product);
		w.WriteUtf8(Version);
	}

	public virtual void Read(BinaryReader r)
	{
		Author = r.ReadUtf8();
		Product = r.ReadUtf8();
		Version = r.ReadUtf8();
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as ApvAddress);
	}

	public bool Equals(ApvAddress o)
	{
		return	o is not null && 
				Author		== o.Author && 
				Product		== o.Product && 
				Version.Equals(o.Version);
	}

	public override int GetHashCode()
	{
		return Author.GetHashCode();
	}

	public static bool operator == (ApvAddress left, ApvAddress right)
	{
		return left is null && right is null || left is not null && left.Equals(right);
	}

	public static bool operator != (ApvAddress left, ApvAddress right)
	{
		return !(left == right);
	}
}
