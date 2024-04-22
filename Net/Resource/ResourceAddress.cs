using System;
using System.IO;
using System.Security.AccessControl;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Uccs.Net
{
	/// <summary>
	///		/domain/resource
	/// </summary>
	 
// 	public enum ResourceType
// 	{
// 		None, Variable, Constant
// 	}

	public class ResourceAddress : IBinarySerializable, IEquatable<ResourceAddress>, IComparable, IComparable<ResourceAddress>
	{
	//	public ResourceType			Type { get; set; }
		public string				Zone { get; set; }
		public string				Domain { get; set; }
		public string				Resource { get; set; }

		public const string			Scheme = "ura";
		public string				Uri => $"{Scheme}:{Zone}{(Zone != null ? "." : null)}{Domain}/{Resource}";
		//public static ResourceType	SchemeToType(string s) => s[2] switch {'v' => ResourceType.Variable, 'c' => ResourceType.Constant};

		public bool					Valid => !string.IsNullOrWhiteSpace(Domain) && !string.IsNullOrWhiteSpace(Resource);

		public ResourceAddress()
		{
		}

		public ResourceAddress(string domain, string resource)
		{
			Domain = domain;
			Resource = resource;
		}

		public override string ToString()
		{
			return $"{Domain}/{Resource}";
		}

		public override bool Equals(object o)
		{
			return o is ResourceAddress a && Equals(a);
		}

		public bool Equals(ResourceAddress o)
		{
			return o is not null && Zone == o.Zone && Domain == o.Domain && Resource == o.Resource;
		}

 		public override int GetHashCode()
 		{
 			return Domain.GetHashCode();
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

			c = Domain.CompareTo(o.Domain);

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
				a.Domain = za.Substring(j+1);
			}
			else
				a.Domain = za;

			a.Resource = v.Substring(r+1);

			return a;
		}

		public static ResourceAddress ParseAR(string v)
		{
			var a = new ResourceAddress();
			
			var i = v.IndexOf('/');

			a.Domain = v.Substring(0, i);
			a.Resource = v.Substring(i+1);

			return a;
		}

		public void Write(BinaryWriter w)
		{
			w.WriteUtf8(Domain);
			w.WriteUtf8(Resource);
		}

		public void Read(BinaryReader r)
		{
			Domain	 = r.ReadUtf8();
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
