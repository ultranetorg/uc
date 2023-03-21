using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UC.Net
{
	public class ReleaseAddress : IBinarySerializable, IEquatable<ReleaseAddress>  
	{
		RealizationAddress		R; /// to disable implicit conversion
		public string			Author => R.Author;
		public string			Product => R.Product;
		public string			Realization => R.Name;
		public Version			Version { get; set; }
		public bool				Valid => R.Valid;

		public static implicit operator RealizationAddress(ReleaseAddress d) => d.R;
		public static implicit operator ProductAddress(ReleaseAddress d) => (ProductAddress)d.R;

		public ReleaseAddress(string author, string product, string platform, Version version)
		{
			R = new(author, product, platform);
			Version = version;
		}

		public ReleaseAddress()
		{
		}

		public override string ToString()
		{
			return $"{R}/{Version}";
		}

		public override bool Equals(object o)
		{
			return o is ReleaseAddress a && Equals(a);
		}

		public bool Equals(ReleaseAddress o)
		{
			return R.Equals(o.R) && Version == o.Version;
		}

 		public override int GetHashCode()
 		{
 			return R.GetHashCode();
 		}

		public static bool operator ==(ReleaseAddress left, ReleaseAddress right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ReleaseAddress left, ReleaseAddress right)
		{
			return !(left == right);
		}

		public static ReleaseAddress Parse(string v)
		{
			var s = v.Split('/');
			var a = new ReleaseAddress();
			a.Parse(s);
			return a;
		}
		
		public void Parse(string[] s)
		{
			R = new();
			R.Parse(s);
			Version = Version.Parse(s[3]);
		}

		public void Write(BinaryWriter w)
		{
			R.Write(w);
			w.Write(Version);
		}

		public void Read(BinaryReader r)
		{
			R = new();
			R.Read(r);
			Version = r.Read<Version>();
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
