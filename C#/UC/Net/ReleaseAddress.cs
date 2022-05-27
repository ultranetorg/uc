using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UC.Net
{
	public class ReleaseAddress : ProductAddress, IEquatable<ReleaseAddress>
	{
		public string			Platform { get; set; }
		public Version			Version { get; set; }

		public override bool	Valid => !string.IsNullOrWhiteSpace(Platform);

		public ReleaseAddress(string author, string product, string platform, Version version) : base(author, product)
		{
			Platform = platform;
			Version = version;
		}

		public ReleaseAddress()
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()}/{Platform}/{Version}";
		}

		public override bool Equals(object o)
		{
			return o is ReleaseAddress a && a.Equals(o);
		}

		public bool Equals(ReleaseAddress o)
		{
			return base.Equals(this) && Version.Equals(o.Version) && Platform.Equals(o.Platform);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public new static ReleaseAddress Parse(string v)
		{
			var s = v.Split('/');
			var a = new ReleaseAddress();
			a.Parse(s);
			return a;
		}
		
		public override void Parse(string[] s)
		{
			base.Parse(s);
	
			Platform = s[2];
			Version = Version.Parse(s[3]);
		}

		public override void Write(BinaryWriter w)
		{
			w.WriteUtf8(Author);
			w.WriteUtf8(Product);
			w.WriteUtf8(Platform);
			w.Write(Version);
		}

		public override void Read(BinaryReader r)
		{
			Author = r.ReadUtf8();
			Product = r.ReadUtf8();
			Platform = r.ReadUtf8();
			Version = r.ReadVersion();
		}
		
		public static bool operator == (ReleaseAddress left, ReleaseAddress right)
		{
			return left.Equals(right);
		}

		public static bool operator != (ReleaseAddress left, ReleaseAddress right)
		{
			return !(left == right);
		}
	}

	public class ReleaseAddressJsonConverter : JsonConverter<ReleaseAddress>
	{
		public override ReleaseAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return ReleaseAddress.Parse(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, ReleaseAddress value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}
