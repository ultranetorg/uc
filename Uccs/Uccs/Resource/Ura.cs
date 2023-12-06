using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Uccs.Net
{
	/// <summary>
	/// author.resource:release/path
	/// </summary>

	public class Ura : IBinarySerializable, IEquatable<Ura> 
	{
		public string		Author { get; set; }
		public string		Resource { get; set; }
		public const char	Separator = '.';

		public bool		Valid => !string.IsNullOrWhiteSpace(Author) && !string.IsNullOrWhiteSpace(Resource);

		public Ura(string author, string resource)
		{
			Author = author;
			Resource = resource;
		}

		public Ura()
		{
		}

		public override string ToString()
		{
			return $"{Author}{Separator}{Resource}";
		}

		public override bool Equals(object o)
		{
			return o is Ura a && Equals(a);
		}

		public bool Equals(Ura o)
		{
			return Author == o.Author && Resource == o.Resource;
		}

 		public override int GetHashCode()
 		{
 			return Author.GetHashCode();
 		}

		public int CompareTo(object obj)
		{
			return CompareTo(obj as Ura);
		}

		public int CompareTo(Ura other)
		{
			if(Author.CompareTo(other.Author) != 0)
				return Author.CompareTo(other.Author);

			return Resource.CompareTo(other.Resource);
		}

		public static bool operator == (Ura left, Ura right)
		{
			return left is null && right is null || left is not null && right is not null && left.Equals(right);
		}

		public static bool operator != (Ura left, Ura right)
		{
			return !(left == right);
		}

		public static Ura Parse(string v)
		{
			var s = v.IndexOf(Separator);
			var a = new Ura();
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

	public class ReleaseAddressJsonConverter : JsonConverter<Ura>
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
