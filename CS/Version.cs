﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs;

public class Version : IEquatable<Version>, IComparable, IBinarySerializable, ITextSerialisable
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
		var v = new Version();

		v.Read(s);

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

	public void Read(string text)
	{
		var c = text.Split('.').ToArray();

		Era = int.Parse(c[0]);
				
		if(c.Length > 1)
			Upgrade = int.Parse(c[1]);

		if(c.Length > 2)
			Bugfix = int.Parse(c[2]);
	}

	public static bool operator < (Version a, Version b)
	{
		if(a.Era != b.Era)
			return a.Era < b.Era;

		if(a.Upgrade != b.Upgrade)
			return a.Upgrade < b.Upgrade;

		return a.Bugfix < b.Bugfix;
	}

	public static bool operator > (Version a, Version b)
	{
		if(a.Era != b.Era)
			return a.Era > b.Era;

		if(a.Upgrade != b.Upgrade)
			return a.Upgrade > b.Upgrade;

		return a.Bugfix > b.Bugfix;
	}

	public static bool operator == (Version left, Version right)
	{
		return left is null && right is null || left is not null && right is not null && left.Equals(right);
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
