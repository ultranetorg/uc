using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UC.Net
{
	public class PackageAddress : IBinarySerializable, IEquatable<PackageAddress>
	{
		ReleaseAddress			Release;
		public string			Author => Release.Author;
		public string			Product => Release.Product;
		public string			Platform => Release.Platform;
		public Version			Version => Release.Version;
		public Distributive		Distributive { get; set; }
	
		public static implicit operator ProductAddress(PackageAddress a) => a.Release;
		public static implicit operator RealizationAddress(PackageAddress a) => a.Release;
		public static implicit operator ReleaseAddress(PackageAddress a) => a.Release;

		public PackageAddress(string author, string product, string platform, Version version, Distributive distribution)
		{
			Release = new(author, product, platform, version);
			Distributive = distribution;
		}
	
		public PackageAddress(ReleaseAddress release, Distributive distribution)
		{
			Release = new(release.Author, release.Product, release.Platform, release.Version);
			Distributive = distribution;
		}
	
		public PackageAddress()
		{
		}

		public static PackageAddress Parse(string v)
		{
			var s = v.Split('/');
			var a = new PackageAddress();
			a.Parse(s);
			return a;
		}

		public override string ToString()
		{
			return Release.ToString() + "/" + Distributive.ToString().ToLower()[0];
		}

		public void Parse(string[] s)
		{
			Release = new();
			Release.Parse(s);
			Distributive = s[4] == "c" ? Distributive.Complete : Net.Distributive.Incremental;
		}

		public void Write(BinaryWriter w)
		{
			Release.Write(w);
			w.Write((byte)Distributive);
		}

		public void Read(BinaryReader r)
		{
			Release = new();
			Release.Read(r);
			Distributive = (Distributive)r.ReadByte();
		}

		public override bool Equals(object obj)
		{
			return obj is PackageAddress p && Equals(p);
		}

		public bool Equals(PackageAddress other)
		{
			return Release.Equals(other.Release) && Distributive == other.Distributive;
		}

 		public override int GetHashCode()
 		{
 			return Release.GetHashCode();
 		}

		public static bool operator ==(PackageAddress left, PackageAddress right)
		{
			return (left is null && right is null) || (left is not null && right is not null && left.Equals(right));
		}

		public static bool operator !=(PackageAddress left, PackageAddress right)
		{
			return !(left == right);
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
}
