using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UC.Net
{
	public enum Distributive
	{
		Null, Complete = 0b01, Incremental = 0b10
	}

	public enum VersionQuery
	{
		Null, Exact, Latest, Previous
	}

	public class ReleaseQuery
	{
		public RealizationAddress	Realization = new();
		public Version				Version { get; set; } 
		public VersionQuery			VersionQuery { get; set; } 
		public string				Channel { get; set; } 

		public string Author
		{ 
			set { Realization.Author = value; } 
			get { return Realization.Author; } 
		}

		public string Product
		{ 
			set { Realization.Product = value; } 
			get { return Realization.Product; } 
		}

		public string Platform
		{ 
			set { Realization.Name = value; } 
			get { return Realization.Name; } 
		}

		public ReleaseQuery(string author, string product, string platform, Version version, VersionQuery versionQuery, string channel)
		{
			Realization = new(author, product, platform);
			Version = version;
			VersionQuery = versionQuery;
			Channel = channel;
		}

		public ReleaseQuery(RealizationAddress realization, Version version, VersionQuery versionQuery, string channel)
		{
			Realization = new(realization.Author, realization.Product, realization.Name);
			Version = version;
			VersionQuery = versionQuery;
			Channel = channel;
		}

		public ReleaseQuery()
		{
		}

 		public override string ToString()
 		{
 			return $"{Realization} {Version} {VersionQuery} {Channel}";
 		}
 
 		public static ReleaseQuery Parse(string v)
 		{
 			var s = v.Split('/');
 			var a = new ReleaseQuery();
 			a.Parse(s);
 			return a;
 		}
 				
 		public void Parse(string[] s)
 		{
 			Realization = new();
 			Realization.Parse(s);
 
			Version = Version.Parse(s[3]);
 			VersionQuery = Enum.Parse<VersionQuery>(s[4]);
 			Channel = s[5];
 		}

		public bool Match(ReleaseAddress address)
		{
			throw new NotImplementedException();
		}

// 		public void Write(BinaryWriter w)
// 		{
// 			Realization.Write(w);
// 			w.Write(Version);
// 			w.Write((byte)VersionQuery);
// 			w.WriteUtf8(Channel);
// 		}
// 
// 		public void Read(BinaryReader r)
// 		{
// 			Realization = new();
// 			Realization.Read(r);
// 			VersionQuery = (VersionQuery)r.ReadByte();
// 			Channel = r.ReadUtf8();
// 		}
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
