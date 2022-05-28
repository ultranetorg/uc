using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UC.Net
{
	public enum Distribution
	{
		Null, Complete, Incremental
	}

	public enum VersionQuery
	{
		Null, Latest
	}

	public class ReleaseQuery : ReleaseAddress
	{
		public VersionQuery			VersionQuery { get; set; } 
		public string				Channel { get; set; } 

		public override bool		Valid => true;

		public ReleaseQuery(string author, string product, string platform, Version version, VersionQuery versionQuery, string channel) : base(author, product, platform, version)
		{
			VersionQuery = versionQuery;
			Channel = channel;
		}

		public ReleaseQuery()
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()}/{VersionQuery}/{Channel}";
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
			Channel = s[5];
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

	public class PackageAddress : ReleaseAddress
	{
		public Distribution			Distribution { get; set; }

		public PackageAddress(string author, string product, Version version, string platform, Distribution distribution) : base(author, product, platform, version)
		{
			Distribution = distribution;
		}

		public PackageAddress()
		{
		}
	}

	public class DownloadPackageRequest : PackageAddress
	{
		public long					Offset { get; set; }
		public long					Length { get; set; }

		public DownloadPackageRequest(string author, string product, Version version, string platform, Distribution distribution, long offset, long length) : base(author, product, version, platform, distribution)
		{
			Distribution = distribution;
			Offset = offset;
			Length = length;
		}

		public DownloadPackageRequest()
		{
		}
// 
// 		public override string ToString()
// 		{
// 			return $"{base.ToString()}/{Distribution.ToString().ToLower()[0]}/{Offset}/{Length}";
// 		}
// 
// 		public new static ReleaseDownloadRequest Parse(string v)
// 		{
// 			var s = v.Split('/');
// 			var a = new ReleaseDownloadRequest();
// 			a.Parse(s);
// 			return a;
// 		}
// 				
// 		public override void Parse(string[] s)
// 		{
// 			base.Parse(s);
// 			
// 			Distribution = s[6] switch {"c" => ReleaseDistribution.Complete,
// 										"i"	=> ReleaseDistribution.Incremental,
// 										_	=> throw new IntegrityException("Unknown ReleaseDistribution")};
// 
// 			Offset = long.Parse(s[7]);
// 			Length = long.Parse(s[8]);
// 		}
	}
	
//	public class ReleaseDownloadRequestJsonConverter : JsonConverter<ReleaseDownloadRequest>
//	{
//		public override ReleaseDownloadRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//		{
//			return ReleaseDownloadRequest.Parse(reader.GetString());
//		}
//
//		public override void Write(Utf8JsonWriter writer, ReleaseDownloadRequest value, JsonSerializerOptions options)
//		{
//			writer.WriteStringValue(value.ToString());
//		}
//	}
}
