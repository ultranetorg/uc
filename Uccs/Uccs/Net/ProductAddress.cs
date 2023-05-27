using System;
using System.IO;

namespace Uccs.Net
{
	public class ProductAddress : IEquatable<ProductAddress>, IBinarySerializable
	{
		public string Author { get; set; }
		public string Name { get; set; }

		public virtual bool Valid => !string.IsNullOrWhiteSpace(Author)  && !string.IsNullOrWhiteSpace(Name);

		public ProductAddress(string author, string product)
		{
			Author = author;
			Name = product;
		}

		public ProductAddress()
		{
		}

		public override string ToString()
		{
			return Author + "/" + Name;
		}

		public override bool Equals(object obj)
		{
			return obj is ProductAddress address && Equals(address);
		}

		public bool Equals(ProductAddress other)
		{
			return Author == other.Author && Name == other.Name;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Author, Name);
		}

		public virtual void Parse(string[] s)
		{
			Author = s[0];
			Name = s[1];
		}

		public static ProductAddress Parse(string v)
		{
			var a = new ProductAddress();
			a.Parse(v.Split('/'));
			return a;
		}

		public static bool operator ==(ProductAddress left, ProductAddress right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ProductAddress left, ProductAddress right)
		{
			return !(left == right);
		}
		
		public virtual void Write(BinaryWriter w)
		{
			w.WriteUtf8(Author);
			w.WriteUtf8(Name);
		}

		public virtual void Read(BinaryReader r)
		{
			Author = r.ReadUtf8();
			Name = r.ReadUtf8();
		}
	}
}
