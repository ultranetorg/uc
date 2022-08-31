using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UC.Net
{
	public class ReleaseAddress : RealizationAddress, IEquatable<ReleaseAddress>
	{
		public Version			Version { get; set; }
		public override bool	Valid => base.Valid;

		public ReleaseAddress(string author, string product, string platform, Version version) : base(author, product, platform)
		{
			Version = version;
		}

		public ReleaseAddress()
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()}/{Version}";
		}

		public override bool Equals(object o)
		{
			return o is ReleaseAddress a && Equals(a);
		}

		public bool Equals(ReleaseAddress o)
		{
			return base.Equals(this) && Version.Equals(o.Version);
		}

 		public override int GetHashCode()
 		{
 			return base.GetHashCode(); /// don't change this!
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
			Version = Version.Parse(s[3]);
		}

		public override void Write(BinaryWriter w)
		{
			base.Write(w);
			w.Write(Version);
		}

		public override void Read(BinaryReader r)
		{
			base.Read(r);
			Version = r.ReadVersion();
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
