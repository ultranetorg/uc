using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Rdn
{
	/// <summary>
	///		/domain/resource
	/// </summary>

	// 	public enum ResourceType
	// 	{
	// 		None, Variable, Constant
	// 	}

	public class Ura : IBinarySerializable, IEquatable<Ura>, IComparable, IComparable<Ura>, ITextSerialisable
	{
	//	public ResourceType			Type { get; set; }
		public string				Zone { get; set; }
		public string				Domain { get; set; }
		public string				Resource { get; set; }

		public const string			Scheme = "ura";
		//public string				Uri => $"{Scheme}:{Zone}{(Zone != null ? ":" : null)}{Domain}/{Resource}";
		//public static ResourceType	SchemeToType(string s) => s[2] switch {'v' => ResourceType.Variable, 'c' => ResourceType.Constant};

		public bool					Valid => !string.IsNullOrWhiteSpace(Domain) && !string.IsNullOrWhiteSpace(Resource);

		public Ura()
		{
		}

		public Ura(string domain, string resource)
		{
			Domain = domain;
			Resource = resource;
		}

		public Ura(string zone, string domain, string resource)
		{
			Zone = zone;
			Domain = domain;
			Resource = resource;
		}

		public Ura(Ura a)
		{
			Zone		= a.Zone;
			Domain		= a.Domain;
			Resource	= a.Resource;
		}

		public override string ToString()
		{
			return UAddress.ToString(Scheme, Zone, $"{Domain}/{Resource}");
		}

		public override bool Equals(object o)
		{
			return o is Ura a && Equals(a);
		}

		public bool Equals(Ura o)
		{
			return o is not null && Zone == o.Zone && Domain == o.Domain && Resource == o.Resource;
		}

 		public override int GetHashCode()
 		{
 			return Domain.GetHashCode();
 		}

		public int CompareTo(object obj)
		{
			return CompareTo(obj as Ura);
		}

		public int CompareTo(Ura o)
		{
			var c = Zone.CompareTo(o.Zone);

			if(c != 0)
				return c;

			c = Domain.CompareTo(o.Domain);

			if(c != 0)
				return c;

			return Resource.CompareTo(o.Resource);
		}

		public static bool operator == (Ura left, Ura right)
		{
			return left is null && right is null || left is not null && left.Equals(right);
		}

		public static bool operator != (Ura left, Ura right)
		{
			return !(left == right);
		}


		public void Read(string text)
		{
			Parse(text, out string p, out string z, out string d, out string r);
			Zone = z;
			Domain = d;
			Resource = r;
		}

		public static void Parse(string v, out string protocol, out string zone, out string domain, out string resource)
		{
			int i;
			
			UAddress.Parse(v, out protocol, out zone, out string o);

			var e = o.IndexOfAny([':', '/']);
				
			if(e != -1)
			{
				domain = o.Substring(0, e);
				i = e + 1;
			}
			else
			{
				domain = o;
				i = -1;
			}

			if(i != -1)
				resource = o.Substring(i);
			else
				resource = null;
		}

		public static Ura Parse(string v)
		{
			Parse(v, out var s, out var z, out var d, out var r);

			return new Ura(z, d, r);
		}

		public static Ura ParseAR(string v)
		{
			var a = new Ura();
			
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

	public class UraJsonConverter : JsonConverter<Ura>
	{
		public override Ura Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return Ura.Parse(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, Ura value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}
