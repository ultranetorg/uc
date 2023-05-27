using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net
{
	public class RealizationAddress : IEquatable<RealizationAddress>, IBinarySerializable
	{
		public ProductAddress	Product { get; set; }
		public string			Name { get; set; }
		public bool				Valid => throw new NotImplementedException();

		public static bool		operator== (RealizationAddress a, RealizationAddress b) => a.Equals(b);
		public static bool		operator!= (RealizationAddress a, RealizationAddress b) => !(a == b);

		public RealizationAddress(string author, string product, string platform)
		{
			Product = new (author, product);
			Name = platform;
		}

		public RealizationAddress(ProductAddress product, string platform)
		{
			Product = product;
			Name = platform;
		}

		public RealizationAddress()
		{
		}

		public override string ToString()
		{
			return $"{Product}/{Name}";
		}

		public override bool Equals(object o)
		{
			return o is RealizationAddress a && Equals(a);
		}

		public bool Equals(RealizationAddress o)
		{
			return Product.Equals(o.Product) && Name.Equals(o.Name);
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
			Product	 = new ProductAddress(s[0], s[1]);
			Name = s[2];
		}

		public void Write(BinaryWriter w)
		{
			w.WriteUtf8(Product.Author);
			w.WriteUtf8(Product.Name);
			w.WriteUtf8(Name);
		}

		public void Read(BinaryReader r)
		{
			Product = new (){
								Author	= r.ReadUtf8(),
								Name	= r.ReadUtf8()
							};

			Name = r.ReadUtf8();
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
