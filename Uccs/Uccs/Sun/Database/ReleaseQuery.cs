using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net
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
		public RealizationAddress	Realization { get; set; } 
		public Version				Version { get; set; } 
		public VersionQuery			VersionQuery { get; set; } 
		public string				Channel { get; set; } 

// 		public string Author
// 		{ 
// 			set { Realization.Author = value; } 
// 			get { return Realization.Author; } 
// 		}
// 
// 		public string Product
// 		{ 
// 			set { Realization.Product = value; } 
// 			get { return Realization.Product; } 
// 		}
// 
// 		public string Platform
// 		{ 
// 			set { Realization.Name = value; } 
// 			get { return Realization.Name; } 
// 		}

		public ReleaseQuery(string author, string product, string platform, Version version, VersionQuery versionQuery, string channel)
		{
			Realization = new (author, product, platform);
			Version = version;
			VersionQuery = versionQuery;
			Channel = channel;
		}

		public ReleaseQuery(RealizationAddress realization, Version version, VersionQuery versionQuery, string channel)
		{
			Realization = realization;
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
 
 		public static ReleaseQuery Parse(Xon v)
 		{
 			var a = new ReleaseQuery();

 			a.Realization	= RealizationAddress.Parse(v.GetString("address"));
  			a.VersionQuery	= Enum.Parse<VersionQuery>(v.GetString("version"));
			a.Channel		= v.GetString("channel");

 			return a;
 		}
 				
		public bool Match(ReleaseAddress address)
		{
			throw new NotImplementedException();
		}
	}
}
