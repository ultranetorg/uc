using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Uccs.Net.TableBase;

namespace Uccs.Net;

public class StringId : BaseId
{
	public string		K;
	
	public override int B
	{
		get => BucketBase.FromBytes(Encoding.UTF8.GetBytes(K.PadRight(3, '\0'), 0, 3)); 
		set => throw new NotSupportedException();
	}

	public StringId()
	{
	}

	public StringId(string k)
	{
		K = k;
	}

	public override string ToString()
	{
		return K;
	}

	public override int GetHashCode()
	{
		return B.GetHashCode();
	}

	public override void Read(BinaryReader reader)
	{
		K = reader.ReadUtf8();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteUtf8(K);
	}

	public override bool Equals(object obj)
	{
		return obj is StringId id && Equals(id);
	}

	public override bool Equals(BaseId a)
	{
		return a is StringId e && K == e.K;
	}

	public override int CompareTo(BaseId a)
	{
		return CompareTo((StringId)a);
	}

	public int CompareTo(StringId a)
	{
		return K.CompareTo(a.K);
	}

	public static bool operator == (StringId left, StringId right)
	{
		return left is null && right is null || left is not null && left.Equals(right); /// object cast is IMPORTANT!!
	}

	public static bool operator != (StringId left, StringId right)
	{
		return !(left == right);
	}
}

public class StringIdJsonConverter : JsonConverter<StringId>
{
	public override StringId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return new StringId(reader.GetString());
	}

	public override void Write(Utf8JsonWriter writer, StringId value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
