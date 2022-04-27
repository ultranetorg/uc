using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UC.Net
{
	public class ReleaseAddress : ProductAddress, IEquatable<ReleaseAddress>
	{
		public string			Platform;
		public Version			Version;

		public override bool	Valid => !string.IsNullOrWhiteSpace(Platform)  && Version.Valid;

		public ReleaseAddress(string author, string product, string platform, Version version) : base(author, product)
		{
			Version = version;
			Platform = platform;
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
			return HashCode.Combine(base.GetHashCode(), Version, Platform);
		}

		public new static ReleaseAddress Parse(string v)
		{
			var s = v.Split('/');
			return new ReleaseAddress(s[0], s[1], s[2], Version.Parse(s[3]));
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
