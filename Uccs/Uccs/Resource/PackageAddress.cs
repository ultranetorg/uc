using System;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	/// <summary>
	///  ultranet://testzone1/uo.app.dotnet7/0123456789ABCDEF
	/// </summary>

	public class PackageAddress : IBinarySerializable, IComparable, IComparable<PackageAddress>, IEquatable<PackageAddress>
	{
		public string			Author		{ get ; set; }
		public string			Product		{ get ; set; }
		public string			Realization { get ; set; }
		public string			Version		{ get ; set; }

		public string			APR => $"{Author}/{Product}/{Realization}";

//		public static implicit operator ResourceAddress (PackageAddress a)
//		{ 
//			return a._Resource ??= new ResourceAddress(ResourceSpace.Permanent, a.Author, $"{a.Product}/{a.Realization}");
//		}

// 		public static implicit operator Ura (PackageAddress a)
// 		{ 
// 			if(a._Ura == null)
// 				a._Ura = new Ura(a.Author, $"{a.Product}{PR}{a.Realization}", a.ToString());
// 			
// 			return a._Ura;
// 		}

		public PackageAddress(string author, string product, string realization, string veriosn)
		{
			Author = author;
			Product = product;
			Realization = realization;
			Version = veriosn;
		}

// 		public PackageAddress(Ura ura)
// 		{
// 			Author = ura.Author;
// 			
// 			var j = ura.Resource.LastIndexOf(Ura.AR);
// 
// 			Product		= ura.Resource.Substring(0, j);
// 			Realization = ura.Resource.Substring(j + 1);
// 			Release		= ReleaseAddress.Parse(ura.Details); 
// 		}

		public PackageAddress(ResourceAddress resource, string version)
		{
			Author		= resource.Author;

			var j = resource.Resource.LastIndexOf('/');
			
			Product		= resource.Resource.Substring(0, j);
			Realization = resource.Resource.Substring(j + 1);
			Version		= version; 
		}

		public PackageAddress()
		{
		}

		public override string ToString()
		{
			return $"{ResourceAddress.Scheme}:{Author}/{Product}/{Realization}/{Version}";
		}

		public static PackageAddress Parse(string v)
		{
			var r = ResourceAddress.Parse(v);
			var a = new PackageAddress();
			var p = r.Resource.Split('/');

			a.Author		= r.Author;
			a.Product		= p[0];
			a.Realization	= p[1];
			a.Version		= p[2]; 

			return a;
		}

		//public static Version ParseVesion(string v)
		//{
		//	return Version.Parse(v.Substring(v.LastIndexOf('/') + 1));
		//}

		public PackageAddress ReplaceVersion(string version)
		{
			return new PackageAddress(Author, Product, Realization, version);
		}

		public int CompareTo(object obj)
		{
			return CompareTo(obj as PackageAddress);
		}

		public int CompareTo(PackageAddress o)
		{
			var a = Author.CompareTo(o.Author);
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
			w.WriteUtf8(Author);
			w.WriteUtf8(Product);
			w.WriteUtf8(Realization);
			w.WriteUtf8(Version);
		}

		public virtual void Read(BinaryReader r)
		{
			Author = r.ReadUtf8();
			Product = r.ReadUtf8();
			Realization = r.ReadUtf8();
			Version = r.ReadUtf8();
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as PackageAddress);
		}

		public bool Equals(PackageAddress o)
		{
			return	o is not null && 
					Author		== o.Author && 
					Product		== o.Product && 
					Realization	== o.Realization && 
					Version.Equals(o.Version);
		}

		public override int GetHashCode()
		{
			return Author.GetHashCode();
		}

		public static bool operator == (PackageAddress left, PackageAddress right)
		{
			return left is null && right is null || left is not null && left.Equals(right);
		}

		public static bool operator != (PackageAddress left, PackageAddress right)
		{
			return !(left == right);
		}
	}
}
