using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net;

public class RawId : BaseId
{
	public byte[]		Bytes;
	
	public override int B
	{
		get => BytesToBucket(Bytes); 
		set => throw new NotSupportedException();
	}

	public RawId()
	{
	}

	public RawId(byte[] k)
	{
		Bytes = k;
	}

	public override string ToString()
	{
		return Bytes.ToHex();
	}

	public override int GetHashCode()
	{
		if(Bytes.Length == 1) return									   Bytes[0];
		if(Bytes.Length == 2) return					   Bytes[1] << 8 | Bytes[0];
		if(Bytes.Length == 3) return	  Bytes[2] << 16 | Bytes[1] << 8 | Bytes[0];
		
		return Bytes[3] << 24 | Bytes[2] << 16 | Bytes[1] << 8 | Bytes[0];
	}

	public override void Read(BinaryReader reader)
	{
		Bytes = reader.ReadBytes();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteBytes(Bytes);
	}

	public override bool Equals(object obj)
	{
		return obj is RawId id && Equals(id);
	}

	public override bool Equals(BaseId a)
	{
		return a is RawId e && Bytes.SequenceEqual(e.Bytes);
	}

	public override int CompareTo(BaseId a)
	{
		return CompareTo((RawId)a);
	}

	public int CompareTo(RawId a)
	{
		return Uccs.Bytes.Comparer.Compare(Bytes, a.Bytes);
	}

	public static bool operator == (RawId left, RawId right)
	{
		return left is null && right is null || left is not null && left.Equals(right); /// object cast is IMPORTANT!!
	}

	public static bool operator != (RawId left, RawId right)
	{
		return !(left == right);
	}
}

public class StringIdJsonConverter : JsonConverter<RawId>
{
	public override RawId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return new RawId(reader.GetString().FromHex());
	}

	public override void Write(Utf8JsonWriter writer, RawId value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
