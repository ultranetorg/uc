using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UC.Net
{
	public class PackageStatus
	{
		public long Length { get; set; }
		public long CompletedLength { get; set; }
	}

	public class PackageAddress : ReleaseAddress, IEquatable<PackageAddress>
	{
		public Distribution Distribution { get; set; }
	
		public PackageAddress(string author, string product, Version version, string platform, Distribution distribution) : base(author, product, platform, version)
		{
			Distribution = distribution;
		}
	
		public PackageAddress(ReleaseAddress release, Distribution distribution) : base(release.Author, release.Product, release.Platform, release.Version)
		{
			Distribution = distribution;
		}
	
		public PackageAddress()
		{
		}

		public new static PackageAddress Parse(string v)
		{
			var s = v.Split('/');
			var a = new PackageAddress();
			a.Parse(s);
			return a;
		}

		public override string ToString()
		{
			return base.ToString() + "/" + Distribution.ToString().ToLower()[0];
		}

		public override void Parse(string[] s)
		{
			base.Parse(s);
			Distribution = s[4] == "c" ? Net.Distribution.Complete : Net.Distribution.Incremental;
		}

		public override void Write(BinaryWriter w)
		{
			base.Write(w);
			w.Write((byte)Distribution);
		}

		public override void Read(BinaryReader r)
		{
			base.Read(r);
			Distribution = (Distribution)r.ReadByte();
		}

		public override bool Equals(object obj)
		{
			return obj is PackageAddress address && base.Equals(obj) && Distribution == address.Distribution;
		}

 		public override int GetHashCode()
 		{
 			return base.GetHashCode(); /// don't change this!
 		}

		public bool Equals(PackageAddress other)
		{
			return Equals(other as object);
		}
	}

	public class PackageAddressJsonConverter : JsonConverter<PackageAddress>
	{
		public override PackageAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return PackageAddress.Parse(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, PackageAddress value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}

	//public class PackageDownload : PackageAddress
	//{
	//	public long					Offset { get; set; }
	//	public long					Length { get; set; }
	//
	//	public PackageDownload(string author, string product, Version version, string platform, Distribution distribution, long offset, long length) : base(author, product, version, platform, distribution)
	//	{
	//		Offset = offset;
	//		Length = length;
	//	}
	//
	//	public PackageDownload()
	//	{
	//	}
	//
	//	public override void Write(BinaryWriter w)
	//	{
	//		throw new NotImplementedException();
	//	}
	//
	//	public override void Read(BinaryReader r)
	//	{
	//		throw new NotImplementedException();
	//	}
	//	// 
	//	// 		public override string ToString()
	//	// 		{
	//	// 			return $"{base.ToString()}/{Distribution.ToString().ToLower()[0]}/{Offset}/{Length}";
	//	// 		}
	//	// 
	//	// 		public new static ReleaseDownloadRequest Parse(string v)
	//	// 		{
	//	// 			var s = v.Split('/');
	//	// 			var a = new ReleaseDownloadRequest();
	//	// 			a.Parse(s);
	//	// 			return a;
	//	// 		}
	//	// 				
	//	// 		public override void Parse(string[] s)
	//	// 		{
	//	// 			base.Parse(s);
	//	// 			
	//	// 			Distribution = s[6] switch {"c" => ReleaseDistribution.Complete,
	//	// 										"i"	=> ReleaseDistribution.Incremental,
	//	// 										_	=> throw new IntegrityException("Unknown ReleaseDistribution")};
	//	// 
	//	// 			Offset = long.Parse(s[7]);
	//	// 			Length = long.Parse(s[8]);
	//	// 		}
	//}

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
