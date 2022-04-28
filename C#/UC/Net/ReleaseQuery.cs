using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UC.Net
{
	public enum ReleaseDistribution
	{
		Null, Complete, Incremental
	}

	public enum VersionQuery
	{
		Null, Exact, Latest
	}

	public class ReleaseQuery : ReleaseAddress
	{
		public VersionQuery			VersionQuery { get; set; } 
		public string				Stage { get; set; } 
		public string				Localization { get; set; } // empty means default

		public override bool		Valid => true;

		public ReleaseQuery(string author, string product, string platform, Version version, VersionQuery versionQuery, string stage, string localization) : base(author, product, platform, version)
		{
			VersionQuery = versionQuery;
			Stage = stage;
			Localization = localization;
		}

		public ReleaseQuery()
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()}/{VersionQuery}/{Stage}/{Localization}";
		}

		public new static ReleaseQuery Parse(string v)
		{
			var s = v.Split('/');
			var a = new ReleaseQuery();
			a.Parse(s);
			return a;
		}
				
		public override void Parse(string[] s)
		{
			base.Parse(s);

			VersionQuery = Enum.Parse<VersionQuery>(s[4]);
			Stage = s[5];
			Platform = s[6];
		}

		public bool Match(ReleaseAddress address)
		{
			throw new NotImplementedException();
		}
	}
	
	public class ReleaseQueryJsonConverter : JsonConverter<ReleaseQuery>
	{
		public override ReleaseQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return ReleaseQuery.Parse(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, ReleaseQuery value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}

	public class ReleaseDownloadRequest : ReleaseAddress
	{
		public string				Localization { get; set; } // empty means default
		public ReleaseDistribution	Distribution { get; set; }
		public long					Offset { get; set; }
		public long					Length { get; set; }

		public ReleaseDownloadRequest(string author, string product, string platform, Version version, string localization, ReleaseDistribution distribution, long offset, long length) : base(author, product, platform, version)
		{
			Localization = localization;
			Distribution = distribution;
			Offset = offset;
			Length = length;
		}

		public ReleaseDownloadRequest()
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()}/{Localization}/{Distribution.ToString().ToLower()[0]}/{Offset}/{Length}";
		}

		public new static ReleaseDownloadRequest Parse(string v)
		{
			var s = v.Split('/');
			var a = new ReleaseDownloadRequest();
			a.Parse(s);
			return a;
		}
				
		public override void Parse(string[] s)
		{
			base.Parse(s);
			
			Localization = s[4];
			Distribution = s[5] switch {"c" => ReleaseDistribution.Complete,
										"i"	=> ReleaseDistribution.Incremental,
										_	=> throw new IntegrityException("Unknown ReleaseDistribution")};

			Offset = long.Parse(s[6]);
			Length = long.Parse(s[7]);
		}
	}
	
	public class ReleaseDownloadRequestJsonConverter : JsonConverter<ReleaseDownloadRequest>
	{
		public override ReleaseDownloadRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return ReleaseDownloadRequest.Parse(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, ReleaseDownloadRequest value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}
