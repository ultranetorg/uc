using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UC.Net
{
	public class RealizationAddress : ProductAddress, IEquatable<RealizationAddress>
	{
		public string			Realization { get; set; }
		public override bool	Valid => !string.IsNullOrWhiteSpace(Realization);

		public RealizationAddress(string author, string product, string platform) : base(author, product)
		{
			Realization = platform;
		}

		public RealizationAddress()
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()}/{Realization}";
		}

		public override bool Equals(object o)
		{
			return o is RealizationAddress a && Equals(a);
		}

		public bool Equals(RealizationAddress o)
		{
			return base.Equals(o) && Realization.Equals(o.Realization);
		}

 		public override int GetHashCode()
 		{
 			return base.GetHashCode(); /// don't change this!
 		}

		public new static RealizationAddress Parse(string v)
		{
			var s = v.Split('/');
			var a = new RealizationAddress();
			a.Parse(s);
			return a;
		}
		
		public override void Parse(string[] s)
		{
			base.Parse(s);
			Realization = s[2];
		}

		public override void Write(BinaryWriter w)
		{
			base.Write(w);
			w.WriteUtf8(Realization);
		}

		public override void Read(BinaryReader r)
		{
			base.Read(r);
			Realization = r.ReadUtf8();
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
