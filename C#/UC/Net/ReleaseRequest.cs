using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UC.Net
{
	public class ReleaseRequest : ProductAddress
	{
		public string			Platform;
		public Version			Version;
		public string			Stage; 
		public string			Localization; // empty means default

		public override bool	Valid => !string.IsNullOrWhiteSpace(Platform)  && Version.Valid;

		public ReleaseRequest(string author, string product, string platform, Version version) : base(author, product)
		{
			Version = version;
			Platform = platform;
		}

		public override string ToString()
		{
			return $"{base.ToString()}/{Platform}/{Version}/{Stage}/{Localization}";
		}

		public new static ReleaseRequest Parse(string v)
		{
			var s = v.Split('/');
			return new ReleaseRequest(s[0], s[1], s[2], Version.Parse(s[3]));
		}

		public bool Match(ReleaseAddress address)
		{
			throw new NotImplementedException();
		}
	}

	public class ReleaseRequestJsonConverter : JsonConverter<ReleaseRequest>
	{
		public override ReleaseRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return ReleaseRequest.Parse(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, ReleaseRequest value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}
