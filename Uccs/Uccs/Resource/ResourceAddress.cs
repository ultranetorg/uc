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
	 
// 	public enum ResourceType
// 	{
// 		None, Variable, Constant
// 	}

	public class ResourceAddress : IBinarySerializable, IEquatable<ResourceAddress>, IComparable, IComparable<ResourceAddress>
	{
	//	public ResourceType			Type { get; set; }
		public string				Zone { get; set; }
		public string				Author { get; set; }
		public string				Resource { get; set; }

		public const string			Scheme = "ura";
		public string				Uri => $"{Scheme}:{Zone}{(Zone != null ? "." : null)}{Author}/{Resource}";
		//public static ResourceType	SchemeToType(string s) => s[2] switch {'v' => ResourceType.Variable, 'c' => ResourceType.Constant};

		public bool					Valid => !string.IsNullOrWhiteSpace(Author) && !string.IsNullOrWhiteSpace(Resource);

		public ResourceAddress()
		{
		}

		public ResourceAddress(string author, string resource)
		{
			Author = author;
			Resource = resource;
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
			return Zone == o.Zone && Author == o.Author && Resource == o.Resource;
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
			var c = Zone.CompareTo(o.Zone);

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
			
			var d = v.IndexOf(':');

			//if(i != -1)
			//	a.Type = SchemeToType(v.Substring(0, i));

			var r = d == -1 ? v.IndexOf('/') : v.IndexOf('/', d+1);
			
			var za = v.Substring(d+1, r-d-1);
			var j = za.IndexOf('.');
				
			if(j != -1)
			{
				a.Zone = za.Substring(0, j);
				a.Author = za.Substring(j+1);
			}
			else
				a.Author = za;

			a.Resource = v.Substring(r+1);

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
			w.WriteUtf8(Author);
			w.WriteUtf8(Resource);
		}

		public void Read(BinaryReader r)
		{
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
