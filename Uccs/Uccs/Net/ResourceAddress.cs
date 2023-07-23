using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Uccs.Net
{
	/// <summary>
	///  uo-app-ms.dotnet7-0.0.0
	///  ultranet://testnet1/uo.app/ms.dotnet7/0.0.0
	/// </summary>

	public class ResourceAddress : IBinarySerializable, IEquatable<ResourceAddress>, IComparable, IComparable<ResourceAddress>
	{
		public string	Author { get; set; }
		public string	Resource { get; set; }

		public bool		Valid => !string.IsNullOrWhiteSpace(Author) && !string.IsNullOrWhiteSpace(Resource);

		public ResourceAddress(string author, string resource)
		{
			Author = author;
			Resource = resource;
		}

		public ResourceAddress()
		{
		}

		public override string ToString()
		{
			return $"{Author}/{Resource}";
		}

		public override bool Equals(object o)
		{
			return o is ResourceAddress a && Equals(a);
		}

		public bool Equals(ResourceAddress o)
		{
			return Author == o.Author && Resource == o.Resource;
		}

 		public override int GetHashCode()
 		{
 			return Author.GetHashCode();
 		}

		public int CompareTo(object obj)
		{
			return CompareTo(obj as ResourceAddress);
		}

		public int CompareTo(ResourceAddress other)
		{
			if(Author.CompareTo(other.Author) != 0)
				return Author.CompareTo(other.Author);

			return Resource.CompareTo(other.Resource);
		}

		public static bool operator == (ResourceAddress left, ResourceAddress right)
		{
			return left is null && right is null || left is not null && right is not null && left.Equals(right);
		}

		public static bool operator != (ResourceAddress left, ResourceAddress right)
		{
			return !(left == right);
		}

		public static ResourceAddress Parse(string v)
		{
			var s = v.IndexOf('/');
			var a = new ResourceAddress();
			a.Author = v.Substring(0, s);
			a.Resource = v.Substring(s + 1);
			return a;
		}

		public void Write(BinaryWriter w)
		{
			w.WriteUtf8(Author);
			w.WriteUtf8(Resource);
		}

		public void Read(BinaryReader r)
		{
			Author = r.ReadUtf8();
			Resource = r.ReadUtf8();
		}
	}

	public class ReleaseAddressJsonConverter : JsonConverter<ResourceAddress>
	{
		public override ResourceAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return ResourceAddress.Parse(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, ResourceAddress value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}
