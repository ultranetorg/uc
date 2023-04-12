using System;
using System.IO;

namespace Uccs.Net
{
	public class PlatformAddress : IEquatable<PlatformAddress>, IBinarySerializable
	{
		public string Author { get; set; }
		public string Name { get; set; }

		public virtual bool Valid => !string.IsNullOrWhiteSpace(Author)  && !string.IsNullOrWhiteSpace(Name);

		public PlatformAddress(string author, string product)
		{
			Author = author;
			Name = product;
		}

		public PlatformAddress()
		{
		}

		public override string ToString()
		{
			return Author + '.' + Name;
		}

		public override bool Equals(object obj)
		{
			return obj is PlatformAddress address && Equals(address);
		}

		public bool Equals(PlatformAddress other)
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

		public static PlatformAddress Parse(string v)
		{
			var a = new PlatformAddress();
			a.Parse(v.Split('.'));
			return a;
		}

		public static bool operator ==(PlatformAddress left, PlatformAddress right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(PlatformAddress left, PlatformAddress right)
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
