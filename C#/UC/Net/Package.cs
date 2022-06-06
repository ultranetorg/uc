using System;
using System.Collections.Generic;
using System.IO;

namespace UC.Net
{
	public class PackageAddress : ReleaseAddress, IEquatable<PackageAddress>
	{
		public Distribution			Distribution { get; set; }
	
		public PackageAddress(string author, string product, Version version, string platform, Distribution distribution) : base(author, product, platform, version)
		{
			Distribution = distribution;
		}
	
		public PackageAddress()
		{
		}

		public override void Write(BinaryWriter w)
		{
			throw new NotImplementedException();
		}

		public override void Read(BinaryReader r)
		{
			throw new NotImplementedException();
		}

		public override bool Equals(object obj)
		{
			return obj is PackageAddress address && base.Equals(obj) && Distribution == address.Distribution;
		}

// 		public override int GetHashCode()
// 		{
// 			return HashCode.Combine(base.GetHashCode(), Distribution);
// 		}

		public bool Equals(PackageAddress other)
		{
			return Equals(other as PackageAddress);
		}
	}

	public class PackageDownload : PackageAddress
	{
		public long					Offset { get; set; }
		public long					Length { get; set; }
	
		public PackageDownload(string author, string product, Version version, string platform, Distribution distribution, long offset, long length) : base(author, product, version, platform, distribution)
		{
			Offset = offset;
			Length = length;
		}
	
		public PackageDownload()
		{
		}

		public override void Write(BinaryWriter w)
		{
			throw new NotImplementedException();
		}

		public override void Read(BinaryReader r)
		{
			throw new NotImplementedException();
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
