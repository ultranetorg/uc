using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Uccs.Net
{
	/// <summary>
	/// author/resource/0123456789abcdef...
	/// </summary>

	public class ReleaseAddress : IBinarySerializable, IEquatable<ReleaseAddress>, IComparable, IComparable<ReleaseAddress>
	{
		public string	Author { get; set; }
		public string	Resource { get; set; }
		public byte[]	Hash { get; set; }

		public bool		Valid => !string.IsNullOrWhiteSpace(Author) && !string.IsNullOrWhiteSpace(Resource) && Hash?.Length > 0;

		public ReleaseAddress(string author, string resource, byte[] hash)
		{
			Author = author;
			Resource = resource;
			Hash = hash;
		}

		public ReleaseAddress()
		{
		}

		public override string ToString()
		{
			return $"{Author}/{Resource}#{Hash.ToHex()}";
		}

		public override bool Equals(object o)
		{
			return o is ReleaseAddress a && Equals(a);
		}

		public bool Equals(ReleaseAddress o)
		{
			return Author == o.Author && Resource == o.Resource && Hash.SequenceEqual(o.Hash);
		}

 		public override int GetHashCode()
 		{
 			return Author.GetHashCode();
 		}

		public int CompareTo(object obj)
		{
			return CompareTo(obj as ReleaseAddress);
		}

		public int CompareTo(ReleaseAddress other)
		{
			if(Author.CompareTo(other.Author) != 0)
				return Author.CompareTo(other.Author);

			return Resource.CompareTo(other.Resource);
		}

		public static bool operator == (ReleaseAddress left, ReleaseAddress right)
		{
			return left is null && right is null || left is not null && right is not null && left.Equals(right);
		}

		public static bool operator != (ReleaseAddress left, ReleaseAddress right)
		{
			return !(left == right);
		}

		public static ReleaseAddress Parse(string v)
		{
			var s = v.IndexOf('/');
			var a = new ReleaseAddress();
			a.Author = v.Substring(0, s);
			a.Resource = v.Substring(s + 1);
			
			s = v.IndexOf('#', s + 1);
			a.Hash = v.Substring(s + 1).HexToByteArray();
			return a;
		}

		public void Write(BinaryWriter w)
		{
			w.WriteUtf8(Author);
			w.WriteUtf8(Resource);
			w.Write(Hash);
		}

		public void Read(BinaryReader r)
		{
			Author = r.ReadUtf8();
			Resource = r.ReadUtf8();
			Hash = r.ReadHash();
		}
	}

	public class ReleaseAddressJsonConverter : JsonConverter<ReleaseAddress>
	{
		public override ReleaseAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return ReleaseAddress.Parse(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, ReleaseAddress value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}
