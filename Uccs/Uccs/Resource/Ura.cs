using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net
{
	/// <summary>
	/// author.resource/details
	/// </summary>

	public class Ura : IBinarySerializable, IEquatable<Ura> 
	{
		public string		Author { get; set; }
		public string		Resource { get; set; }
		public string		Details { get; set; }

		public const char	RSeparator = '.';
		public const char	DSeparator = '/';

		public bool			Valid => !string.IsNullOrWhiteSpace(Author) && !string.IsNullOrWhiteSpace(Resource);

		public Ura(string author, string resource, string details)
		{
			Author = author;
			Resource = resource;
			Details = details;
		}

		public Ura()
		{
		}

		public override string ToString()
		{
			return $"{Author}{RSeparator}{Resource}{(string.IsNullOrEmpty(Details) ? null : (DSeparator + Details))}";
		}

		public override bool Equals(object o)
		{
			return o is Ura a && Equals(a);
		}

		public bool Equals(Ura o)
		{
			return Author == o.Author && Resource == o.Resource && Details == o.Details;
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

			if(Resource.CompareTo(other.Resource) != 0)
				return Resource.CompareTo(other.Resource);

			return Details.CompareTo(other.Details);
		}

		public static bool operator == (Ura a, Ura b)
		{
			return a is null && b is null || a is not null && b is not null && a.Equals(b);
		}

		public static bool operator != (Ura left, Ura right)
		{
			return !(left == right);
		}

		public static Ura Parse(string v)
		{
			var s = v.IndexOf(RSeparator);
			var a = new Ura();
			a.Author	= v.Substring(0, s);
			a.Resource	= v.Substring(s + 1);

			s = v.IndexOf(DSeparator);

			if(s != -1)
			{
				a.Details	= a.Resource.Substring(s + 1);
				a.Resource	= a.Resource.Substring(0, s);
			} 

			return a;
		}

		public void Write(BinaryWriter w)
		{
			w.WriteUtf8(Author);
			w.WriteUtf8(Resource);
			w.WriteBytes(!string.IsNullOrEmpty(Details) ? Encoding.UTF8.GetBytes(Details) : null);
		}

		public void Read(BinaryReader r)
		{
			Author = r.ReadUtf8();
			Resource = r.ReadUtf8();
			Details = r.ReadBytes() is byte[] b ? Encoding.UTF8.GetString(b) : null;
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
