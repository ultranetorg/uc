using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net
{
	public class RealizationAddress : IEquatable<RealizationAddress>, IBinarySerializable
	{
		public ProductAddress	Product { get; set; }
		//public string			Author { set => ProductAddres.Author = value; get => ProductAddres.Author; }
		//public string			Product { set => ProductAddres.Product = value; get => ProductAddres.Product; }
		public PlatformAddress	Platform { get; set; }
		public bool				Valid => throw new NotImplementedException();

		//public static implicit	operator ProductAddress(RealizationAddress d) => d.Product;
		public static bool		operator ==(RealizationAddress left, RealizationAddress right) => left.Equals(right);
		public static bool		operator !=(RealizationAddress left, RealizationAddress right) => !(left == right);

		public RealizationAddress(string author, string product, string platformauthor, string platformname)
		{
			Product = new (author, product);
			Platform = new (platformauthor, platformname);
		}

		public RealizationAddress(string author, string product, PlatformAddress platform)
		{
			Product = new(author, product);
			Platform = platform;
		}

		public RealizationAddress(ProductAddress product, PlatformAddress platform)
		{
			Product = product;
			Platform = platform;
		}

		public RealizationAddress()
		{
		}

		public override string ToString()
		{
			return $"{Product}/{Platform}";
		}

		public override bool Equals(object o)
		{
			return o is RealizationAddress a && Equals(a);
		}

		public bool Equals(RealizationAddress o)
		{
			return Product.Equals(o.Product) && Platform.Equals(o.Platform);
		}

 		public override int GetHashCode()
 		{
 			return Product.GetHashCode(); /// don't change this!
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
			Product	 = ProductAddress.Parse(s[0]);
			Platform = PlatformAddress.Parse(s[1]);
		}

		public void Write(BinaryWriter w)
		{
			w.WriteUtf8(Product.Author);
			w.WriteUtf8(Product.Name);
			w.WriteUtf8(Platform.Author);
			w.WriteUtf8(Platform.Name);
		}

		public void Read(BinaryReader r)
		{
			Product = new ();
			Platform = new ();

			Product.Author	= r.ReadUtf8();
			Product.Name	= r.ReadUtf8();
			Platform.Author	= r.ReadUtf8();
			Platform.Name	= r.ReadUtf8();
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
