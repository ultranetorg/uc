using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using System.Xml.Linq;

namespace Uccs.Net
{
	/// <summary>
	///  uo-app-ms.dotnet7-0.0.0
	///  ultranet://testnet1/uo.app/ms.dotnet7/0.0.0
	/// </summary>

	public class PackageAddress : IBinarySerializable, IComparable, IComparable<PackageAddress>, IEquatable<PackageAddress>
	{
		string	_Author;
		string	_Product;
		string	_Realization;
		Version	_Version;

		public string	Author { get { return _Author;  } set { _Author = value; _Release = null; _String = null; } }
		public string	Product  { get { return _Product;  } set { _Product = value; _Release = null; _String = null; } }
		public string	Realization  { get { return _Realization;  } set{ _Realization = value; _Release = null; _String = null; } }
		public Version	Version { get { return _Version;  } set { _Version = value; _Release = null; _String = null; } }

		public string APR => $"{Author}/{Product}/{Realization}";

		ResourceAddress	_Release;
		string			_String;

		public PackageAddress(string author, string product, string realization, Version version)
		{
			Author = author;
			Product = product;
			Realization = realization;
			Version = version;
		}

		public PackageAddress(ResourceAddress release)
		{
			Author = release.Author;

			var s = release.Resource.Split('/');
			
			Product = s[0];
			Realization = s[1];
			Version = Version.Parse(s[2]);
		}

		public PackageAddress()
		{
		}

		public static implicit operator ResourceAddress (PackageAddress a)
		{ 
			if(a._Release == null)
				a._Release = new ResourceAddress(a.Author, $"{a.Product}/{a.Realization}/{a.Version}");
			
			return a._Release;
		}

		public static bool operator == (PackageAddress left, PackageAddress right)
		{
			return left is null && right is null || left is not null && right is not null && left.Equals(right);
		}

		public static bool operator != (PackageAddress left, PackageAddress right)
		{
			return !(left == right);
		}

		public override string ToString()
		{
			if(_String == null)
				_String = $"{Author}/{Product}/{Realization}/{Version}";

			return _String;
		}

		public static Version ParseVesion(string v)
		{
			return Version.Parse(v.Substring(v.LastIndexOf('/') + 1));
		}

		public static PackageAddress Parse(string v)
		{
			var a = new PackageAddress();

			var s = v.Split('/');
			
			a.Author = s[0];
			a.Product = s[1];
			a.Realization = s[2];
			a.Version = Version.Parse(s[3]);

			return a;
		}

		public int CompareTo(object obj)
		{
			return CompareTo(obj as PackageAddress);
		}

		public int CompareTo(PackageAddress other)
		{
			var a = Author.CompareTo(other.Author);
			if(a != 0)
				return a;

			a = Product.CompareTo(other.Product);
			if(a != 0)
				return a;

			a = Realization.CompareTo(other.Realization);
			if(a != 0)
				return a;

			return Version.CompareTo(other.Version);
		}
		
		public virtual void Write(BinaryWriter w)
		{
			w.WriteUtf8(Author);
			w.WriteUtf8(Product);
			w.WriteUtf8(Realization);
			w.Write(Version);
		}

		public virtual void Read(BinaryReader r)
		{
			Author = r.ReadUtf8();
			Product = r.ReadUtf8();
			Realization = r.ReadUtf8();
			Version = r.Read<Version>();
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as PackageAddress);
		}

		public bool Equals(PackageAddress o)
		{
			return o is not null && Author == o.Author && Product == o.Product && Realization == o.Realization && Version == o.Version;
		}

		public override int GetHashCode()
		{
			return Author.GetHashCode();
		}
	}
}
