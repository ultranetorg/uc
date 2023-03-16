using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UC.Net
{
	public class ReleaseAddress : IBinarySerializable, IEquatable<ReleaseAddress>  
	{
		RealizationAddress		Realization;
		public string			Author => Realization.Author;
		public string			Product => Realization.Product;
		public string			Platform => Realization.Platform;
		public Version			Version { get; set; }
		public bool				Valid => Realization.Valid;

		public static implicit operator RealizationAddress(ReleaseAddress d) => d.Realization;
		public static implicit operator ProductAddress(ReleaseAddress d) => d.Realization;

		public ReleaseAddress(string author, string product, string platform, Version version)
		{
			Realization = new(author, product, platform);
			Version = version;
		}

		public ReleaseAddress()
		{
		}

		public override string ToString()
		{
			return $"{Realization}/{Version}";
		}

		public override bool Equals(object o)
		{
			return o is ReleaseAddress a && Equals(a);
		}

		public bool Equals(ReleaseAddress o)
		{
			return Realization.Equals(o.Realization) && Version == o.Version;
		}

 		public override int GetHashCode()
 		{
 			return Realization.GetHashCode();
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
			Realization = new();
			Realization.Parse(s);
			Version = Version.Parse(s[3]);
		}

		public void Write(BinaryWriter w)
		{
			Realization.Write(w);
			w.Write(Version);
		}

		public void Read(BinaryReader r)
		{
			Realization = new();
			Realization.Read(r);
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
