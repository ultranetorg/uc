using System;
using System.IO;
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
		Null, Exact, Latest
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

		public ReleaseQuery(ReleaseAddress release, VersionQuery versionQuery, string channel) : base(release.Author, release.Product, release.Platform, release.Version)
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

		public override void Write(BinaryWriter w)
		{
			base.Write(w);
			w.Write((byte)VersionQuery);
			w.WriteUtf8(Channel);
		}

		public override void Read(BinaryReader r)
		{
			base.Read(r);
			VersionQuery = (VersionQuery)r.ReadByte();
			Channel = r.ReadUtf8();
		}
	}
// 
// 	public class ReleaseQueryJsonConverter : JsonConverter<ReleaseQuery>
// 	{
// 		public override ReleaseQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
// 		{
// 			return ReleaseQuery.Parse(reader.GetString());
// 		}
// 
// 		public override void Write(Utf8JsonWriter writer, ReleaseQuery value, JsonSerializerOptions options)
// 		{
// 			writer.WriteStringValue(value.ToString());
// 		}
// 	}


}
