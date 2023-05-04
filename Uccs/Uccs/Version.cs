//using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System;
using Uccs.Net;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Uccs
{
	public class Version : IEquatable<Version>, IComparable, IBinarySerializable
	{
		public int Era;
		public int Upgrade; /// for dependencies ebabled for updating can go up only
		public int Bugfix; /// for dependencies ebabled for updating can go up and down

		public string AB => $"{Era}.{Upgrade}";
		public string ABC => AB + $".{Bugfix}";

		public readonly static Version Zero = new Version(0, 0, 0);

		public Version()
		{
		}

		public Version(int era, int upgrade, int bugfix)
		{
			Era = era;
			Upgrade = upgrade;
			Bugfix = bugfix;
		}

		public override string ToString()
		{
			return $"{Era}.{Upgrade}.{Bugfix}";
		}

		public static Version Parse(string s)
		{
			var c = s.Split('.').ToArray();

			var v = new Version();

			v.Era = int.Parse(c[0]);
					
			if(c.Length > 1)
				v.Upgrade = int.Parse(c[1]);

			if(c.Length > 2)
				v.Bugfix = int.Parse(c[2]);

			return v;
		}

// 		public static Version Read(BinaryReader r)
// 		{
// 			return new Version
// 					{	
// 						Era = r.ReadUInt16(), 
// 						Generation = r.ReadUInt16(), 
// 						Release = r.ReadUInt16(), 
// 						Build = r.ReadUInt16()
// 					};
// 		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Version);
		}

		public bool Equals(Version other)
		{
			return other != null &&
				   Era == other.Era &&
				   Upgrade == other.Upgrade &&
				   Bugfix == other.Bugfix;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Era, Upgrade, Bugfix);
		}

		public int CompareTo(object obj)
		{
			var o = obj as Version;

			if(o > this)
				return -1;
			else if(o < this)
				return 1;
			else
				return 0;
		}

		public void Read(BinaryReader r)
		{
			Era = r.Read7BitEncodedInt();
			Upgrade = r.Read7BitEncodedInt(); 
			Bugfix = r.Read7BitEncodedInt();
		}

		public void Write(BinaryWriter w)
		{
			w.Write7BitEncodedInt(Era);
			w.Write7BitEncodedInt(Upgrade);
			w.Write7BitEncodedInt(Bugfix);
		}

		public static bool operator < (Version a, Version b)
		{
			if(a.Era < b.Era)
				return true;

			if(a.Upgrade < b.Upgrade)
				return true;

			if(a.Bugfix < b.Bugfix)
				return true;

			return false;
		}

		public static bool operator > (Version a, Version b)
		{
			if(a.Era > b.Era)
				return true;

			if(a.Upgrade > b.Upgrade)
				return true;

			if(a.Bugfix > b.Bugfix)
				return true;

			return false;
		}

		public static bool operator == (Version left, Version right)
		{
			return EqualityComparer<Version>.Default.Equals(left, right);
		}

		public static bool operator !=(Version left, Version right)
		{
			return !(left == right);
		}

		public static bool operator <= (Version a, Version b)
		{
			return a < b || a == b;
		}

		public static bool operator >= (Version a, Version b)
		{
			return a > b || a == b;
		}
	}

 	public class VersionJsonConverter : JsonConverter<Version>
 	{
 		public override Version Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
 		{
 			return Version.Parse(reader.GetString());
 		}
 
 		public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options)
 		{
 			writer.WriteStringValue(value.ToString());
 		}
 	}
}
