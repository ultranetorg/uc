using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net
{
	public class RealizationAddress : IEquatable<RealizationAddress>, IBinarySerializable
	{
		ProductAddress	P = new ();
		public string	Author { set => P.Author = value; get => P.Author; }
		public string	Product { set => P.Product = value; get => P.Product; }
		public string	Name { get; set; }
		public bool		Valid => !string.IsNullOrWhiteSpace(Name);

		public static implicit operator ProductAddress(RealizationAddress d) => d.P;

		public static bool operator ==(RealizationAddress left, RealizationAddress right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(RealizationAddress left, RealizationAddress right)
		{
			return !(left == right);
		}

		public RealizationAddress(string author, string product, string name)
		{
			P.Author = author;
			P.Product = product;
			Name = name;
		}

		public RealizationAddress()
		{
		}

		public override string ToString()
		{
			return $"{P}/{Name}";
		}

		public override bool Equals(object o)
		{
			return o is RealizationAddress a && Equals(a);
		}

		public bool Equals(RealizationAddress o)
		{
			return P.Equals(o.P) && Name.Equals(o.Name);
		}

 		public override int GetHashCode()
 		{
 			return P.GetHashCode(); /// don't change this!
 		}

		public static RealizationAddress Parse(string v)
		{
			var s = v.Split('/');
			var a = new RealizationAddress();
			a.Parse(s);
			return a;
		}
		
		public void Parse(string[] s)
		{
			P.Author = s[0];
			P.Product = s[1];
			Name = s[2];
		}

		public void Write(BinaryWriter w)
		{
			w.WriteUtf8(P.Author);
			w.WriteUtf8(P.Product);
			w.WriteUtf8(Name);
		}

		public void Read(BinaryReader r)
		{
			P.Author	= r.ReadUtf8();
			P.Product	= r.ReadUtf8();
			Name		= r.ReadUtf8();
		}
	}

	public class RealizationAddressJsonConverter : JsonConverter<RealizationAddress>
	{
		public override RealizationAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return RealizationAddress.Parse(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, RealizationAddress value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}
