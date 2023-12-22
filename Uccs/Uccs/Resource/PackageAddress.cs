using System;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	/// <summary>
	///  uo-app-ms.dotnet7-0.0.0
	///  ultranet://testnet1/uo.app/ms.dotnet7/0.0.0
	/// </summary>

	public class PackageAddress : IBinarySerializable, IComparable, IComparable<PackageAddress>, IEquatable<PackageAddress>
	{
		public string	Author		{ get { return _Author; }		set { _Author = value;		_Resource = null; _String = null; _Ura = null; } }
		public string	Product		{ get { return _Product; }		set { _Product = value;		_Resource = null; _String = null; _Ura = null; } }
		public string	Realization { get { return _Realization; }	set { _Realization = value; _Resource = null; _String = null; _Ura = null; } }
		public byte[]	Hash		{ get { return _Hash; }			set { _Hash = value;		_Resource = null; _String = null; _Ura = null; } }

		public const char	PR = '.';

		public string	APR => $"{Author}{Ura.AR}{Product}{PR}{Realization}";

		string			_Author;
		string			_Product;
		string			_Realization;
		byte[]			_Hash;
		ResourceAddress	_Resource;
		Ura				_Ura;
		string			_String;


		public static implicit operator ResourceAddress (PackageAddress a)
		{ 
			if(a._Resource == null)
				a._Resource = new ResourceAddress(a.Author, $"{a.Product}{PR}{a.Realization}");
			
			return a._Resource;
		}

		public static implicit operator Ura (PackageAddress a)
		{ 
			if(a._Ura == null)
				a._Ura = new Ura(a.Author, $"{a.Product}{PR}{a.Realization}", a.Hash.ToHex());
			
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

		public PackageAddress(string author, string product, string realization, byte[] hash)
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
			Hash		= ura.Details.FromHex(); 
		}

		public PackageAddress(ResourceAddress release, byte[] hash)
		{
			Author = release.Author;

			var j = release.Resource.LastIndexOf(Ura.AR);

			Product		= release.Resource.Substring(0, j);
			Realization = release.Resource.Substring(j + 1);
			Hash		= hash; 
		}

		public PackageAddress()
		{
		}

		public override string ToString()
		{
			if(_String == null)
				_String = $"{Author}{Ura.AR}{Product}{PR}{Realization}{Ura.RD}{Hash.ToHex()}";

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
			p.Hash			= v.Substring(h + 1).FromHex(); 

			return p;
		}

		//public static Version ParseVesion(string v)
		//{
		//	return Version.Parse(v.Substring(v.LastIndexOf('/') + 1));
		//}

		public PackageAddress ReplaceHash(byte[] hash)
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

			if(Hash is null && o.Hash is not null)
				return -1;
			if(Hash is not null && o.Hash is null)
				return 1;
			else
				return Bytes.Comparer.Compare(Hash, o.Hash);
		}
		
		public virtual void Write(BinaryWriter w)
		{
			w.WriteUtf8(Author);
			w.WriteUtf8(Product);
			w.WriteUtf8(Realization);
			w.WriteBytes(Hash);
		}

		public virtual void Read(BinaryReader r)
		{
			Author = r.ReadUtf8();
			Product = r.ReadUtf8();
			Realization = r.ReadUtf8();
			Hash = r.ReadBytes();
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
					((Hash is null && o.Hash is null) || (Hash is not null && o.Hash is not null && Hash.SequenceEqual(o.Hash)));
		}

		public override int GetHashCode()
		{
			return Author.GetHashCode();
		}
	}
}
