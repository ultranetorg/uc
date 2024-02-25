using System;
using System.IO;
using System.Security.AccessControl;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Uccs.Net
{
	/// <summary>
	///		/author/resource
	/// </summary>
	 
	public enum ResourceType
	{
		None, Variable, Constant
	}

	public class ResourceAddress : IBinarySerializable, IEquatable<ResourceAddress>, IComparable, IComparable<ResourceAddress>
	{
		public ResourceType			Type { get; set; }
		public string				Zone { get; set; }
		public string				Author { get; set; }
		public string				Resource { get; set; }

		public string				Scheme => Type switch {ResourceType.Variable => "upv", ResourceType.Constant => "upc"};
		public static ResourceType	SchemeToType(string s) => s[2] switch {'v' => ResourceType.Variable, 'c' => ResourceType.Constant};

		public bool					Valid => !string.IsNullOrWhiteSpace(Author) && !string.IsNullOrWhiteSpace(Resource);

		public ResourceAddress()
		{
		}

		public ResourceAddress(ResourceType type, string author, string resource)
		{
			Type = type;
			Author = author;
			Resource = resource;
		}

		public override string ToString()
		{
			return $"{Scheme}:{Zone}/{Author}/{Resource}";
		}

		public override bool Equals(object o)
		{
			return o is ResourceAddress a && Equals(a);
		}

		public bool Equals(ResourceAddress o)
		{
			return Type == o.Type && Zone == o.Zone && Author == o.Author && Resource == o.Resource;
		}

 		public override int GetHashCode()
 		{
 			return Author.GetHashCode();
 		}

		public int CompareTo(object obj)
		{
			return CompareTo(obj as ResourceAddress);
		}

		public int CompareTo(ResourceAddress o)
		{
			var c = Type.CompareTo(o.Type); 

			if(c != 0)
				return c;

			c = Zone.CompareTo(o.Zone);

			if(c != 0)
				return c;

			c = Author.CompareTo(o.Author);

			if(c != 0)
				return c;

			return Resource.CompareTo(o.Resource);
		}

		public static bool operator == (ResourceAddress left, ResourceAddress right)
		{
			return left is null && right is null || left is not null && left.Equals(right);
		}

		public static bool operator != (ResourceAddress left, ResourceAddress right)
		{
			return !(left == right);
		}

		public static ResourceAddress Parse(string v)
		{
			var a = new ResourceAddress();
			
			var i = v.IndexOf(':');

			if(i != -1)
				a.Type = SchemeToType(v.Substring(0, i));

			var j = v.IndexOf('/', i+1);
			
			if(i+1 < j)
				a.Zone = v.Substring(i+1, j-i-1);

			i = j;
			j = v.IndexOf('/', i+1);
			a.Author = v.Substring(i+1, j-i-1);

			i = j;
			j = v.IndexOf('/', i+1);
			a.Resource = v.Substring(i+1);

			return a;
		}

		public static ResourceAddress ParseAR(string v)
		{
			var a = new ResourceAddress();
			
			var i = v.IndexOf('/');

			a.Author = v.Substring(0, i);
			a.Resource = v.Substring(i+1);

			return a;
		}

		public void Write(BinaryWriter w)
		{
			w.Write((byte)Type);
			w.WriteUtf8(Author);
			w.WriteUtf8(Resource);
		}

		public void Read(BinaryReader r)
		{
			Type	 = (ResourceType)r.ReadByte();
			Author	 = r.ReadUtf8();
			Resource = r.ReadUtf8();
		}
	}

	public class ResourceAddressJsonConverter : JsonConverter<ResourceAddress>
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
