using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UC.Net
{
	public class VersionAddress : IBinarySerializable, IEquatable<VersionAddress>  
	{
		RealizationAddress		R;
		public string			Author => R.Author;
		public string			Product => R.Product;
		public string			Realization => R.Realization;
		public Version			Version { get; set; }
		public bool				Valid => R.Valid;

		public static implicit operator RealizationAddress(VersionAddress d) => d.R;
		public static implicit operator ProductAddress(VersionAddress d) => d.R;

		public VersionAddress(string author, string product, string platform, Version version)
		{
			R = new(author, product, platform);
			Version = version;
		}

		public VersionAddress()
		{
		}

		public override string ToString()
		{
			return $"{R}/{Version}";
		}

		public override bool Equals(object o)
		{
			return o is VersionAddress a && Equals(a);
		}

		public bool Equals(VersionAddress o)
		{
			return R.Equals(o.R) && Version == o.Version;
		}

 		public override int GetHashCode()
 		{
 			return R.GetHashCode();
 		}

		public static bool operator ==(VersionAddress left, VersionAddress right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(VersionAddress left, VersionAddress right)
		{
			return !(left == right);
		}

		public static VersionAddress Parse(string v)
		{
			var s = v.Split('/');
			var a = new VersionAddress();
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

	public class ReleaseAddressJsonConverter : JsonConverter<VersionAddress>
	{
		public override VersionAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return VersionAddress.Parse(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, VersionAddress value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}
