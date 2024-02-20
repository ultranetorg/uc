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
		public string			Author		{ get { return _Author; }		set { _Author = value;		_Resource = null; _String = null; _Ura = null; } }
		public string			Product		{ get { return _Product; }		set { _Product = value;		_Resource = null; _String = null; _Ura = null; } }
		public string			Realization { get { return _Realization; }	set { _Realization = value; _Resource = null; _String = null; _Ura = null; } }
		public ReleaseAddress	Release		{ get { return _Hash; }			set { _Hash = value;		_Resource = null; _String = null; _Ura = null; } }

		public const char		PR = '.';
		public string			APR => $"{Author}{Ura.AR}{Product}{PR}{Realization}";

		string					_Author;
		string					_Product;
		string					_Realization;
		ReleaseAddress			_Hash;
		ResourceAddress			_Resource;
		Ura						_Ura;
		string					_String;

		public static implicit operator ResourceAddress (PackageAddress a)
		{ 
			if(a._Resource == null)
				a._Resource = new ResourceAddress(a.Author, $"{a.Product}{PR}{a.Realization}");
			
			return a._Resource;
		}

		public static implicit operator Ura (PackageAddress a)
		{ 
			if(a._Ura == null)
				a._Ura = new Ura(a.Author, $"{a.Product}{PR}{a.Realization}", a.ToString());
			
			return a._Ura;
		}

		public static bool operator == (PackageAddress left, PackageAddress right)
		{
			return left is null && right is null || left is not null && right is not null && left.Equals(right);
		}

		public static bool operator != (PackageAddress left, PackageAddress right)
		{
			return !(left == right);
		}

		public PackageAddress(string author, string product, string realization, ReleaseAddress hash)
		{
			_Author = author;
			_Product = product;
			_Realization = realization;
			_Hash = hash;
		}

		public PackageAddress(Ura ura)
		{
			Author = ura.Author;
			
			var j = ura.Resource.LastIndexOf(Ura.AR);

			Product		= ura.Resource.Substring(0, j);
			Realization = ura.Resource.Substring(j + 1);
			Release		= ReleaseAddress.Parse(ura.Details); 
		}

		public PackageAddress(ResourceAddress release, ReleaseAddress hash)
		{
			Author = release.Author;

			var j = release.Resource.LastIndexOf(Ura.AR);

			Product		= release.Resource.Substring(0, j);
			Realization = release.Resource.Substring(j + 1);
			Release		= hash; 
		}

		public PackageAddress()
		{
		}

		public override string ToString()
		{
			if(_String == null)
				_String = $"{Author}{Ura.AR}{Product}{PR}{Realization}{Ura.RD}{Release}";

			return _String;
		}

		public static PackageAddress Parse(string v)
		{
			var h = v.IndexOf(Ura.RD);
			
			var apr = v.Substring(0, h);
			
			var a = apr.IndexOf(Ura.AR);
			var r = apr.LastIndexOf(PR);

			var p = new PackageAddress();

			p.Author		= apr.Substring(0, a);
			p.Product		= apr.Substring(a + 1, r-a-1);
			p.Realization	= apr.Substring(r + 1);
			p.Release		= ReleaseAddress.Parse(v.Substring(h + 1)); 

			return p;
		}

		//public static Version ParseVesion(string v)
		//{
		//	return Version.Parse(v.Substring(v.LastIndexOf('/') + 1));
		//}

		public PackageAddress ReplaceHash(ReleaseAddress hash)
		{
			return new PackageAddress(Author, Product, Realization, hash);
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

			if(Release is null && o.Release is not null)
				return -1;
			if(Release is not null && o.Release is null)
				return 1;
			else
				return Bytes.Comparer.Compare(Release.Raw, o.Release.Raw);
		}
		
		public virtual void Write(BinaryWriter w)
		{
			w.WriteUtf8(Author);
			w.WriteUtf8(Product);
			w.WriteUtf8(Realization);
			w.Write(Release);
		}

		public virtual void Read(BinaryReader r)
		{
			Author = r.ReadUtf8();
			Product = r.ReadUtf8();
			Realization = r.ReadUtf8();
			Release = r.Read<ReleaseAddress>(ReleaseAddress.FromType);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as PackageAddress);
		}

		public bool Equals(PackageAddress o)
		{
			return	o is not null && 
					Author == o.Author && 
					Product == o.Product && 
					Realization == o.Realization && 
					((Release is null && o.Release is null) || (Release is not null && Release.Equals(o.Release)));
		}

		public override int GetHashCode()
		{
			return Author.GetHashCode();
		}
	}
}
