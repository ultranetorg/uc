using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net
{
	/// <summary>
	/// 
	/// ultranet:/author
	/// ultranet:/author/reso/ur/ce (/author/ - корневой)
	/// ultranet:#0123456789ABCDEF
	/// ultranet:$author/product:hhhhhhhhhhhhhhhhhhh:sssssssssssssssssssssssssssssssssssssssssssssss
	/// 
	/// </summary>

	public class Ura : IBinarySerializable, IEquatable<Ura> 
	{
		public string			Author { get; set; }
		public string			Resource { get; set; }
		public ReleaseAddress	Release { get; set; }

		public bool				Valid => !string.IsNullOrWhiteSpace(Author) && !string.IsNullOrWhiteSpace(Resource);

		public Ura()
		{
		}

		public override string ToString()
		{
			if(Author != null)
				if(Resource == null)
					return $"/{Author}";
				else
					return $"/{Author}/{Resource}";
			else
				return $"{Release}";
		}

		public override bool Equals(object o)
		{
			return o is Ura a && Equals(a);
		}

		public bool Equals(Ura o)
		{
			return Author == o.Author && Resource == o.Resource && Release == o.Release;
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

			///return Release.CompareTo(other.Release);
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		public void Write(BinaryWriter w)
		{
			throw new NotImplementedException();
			//w.WriteUtf8(Author);
			//w.WriteUtf8(Resource);
			//w.Write();
		}

		public void Read(BinaryReader r)
		{
			throw new NotImplementedException();
			///Author = r.ReadUtf8();
			///Resource = r.ReadUtf8();
			///Details = r.ReadBytes() is byte[] b ? Encoding.UTF8.GetString(b) : null;
		}
	}

// 	public class ReleaseAddressJsonConverter : JsonConverter<Ura>
// 	{
// 		public override Ura Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
// 		{
// 			return Ura.Parse(reader.GetString());
// 		}
// 
// 		public override void Write(Utf8JsonWriter writer, Ura value, JsonSerializerOptions options)
// 		{
// 			writer.WriteStringValue(value.ToString());
// 		}
// 	}
}
