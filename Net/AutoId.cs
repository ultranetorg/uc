using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net;

public class AutoId : EntityId
{
	public long							I { get; set; }
	public override int					B  => (int)(I % TableBase.BucketBase.CountMax); /// bucket

	public static readonly AutoId		God = new() { I = -1};
	public static readonly AutoId		LastCreated = new () {I = -2};
	public static readonly AutoId		NewUser = new () {I = -3};
	public static readonly AutoId		FreeConst  = new () {I = -4};

	public ulong						ToULong() => (ulong)I;
	public static AutoId				FromULong(ulong l) => new ((long)l);

	public AutoId()
	{
	}

	public AutoId(long e)
	{
		I = e;
	}

	public override string ToString()
	{
		return $"{I}";
	}

	public override int GetHashCode()
	{
		return B;
	}

	public static bool TryParse(string t, out AutoId entity)
	{
		var r = long.TryParse(t, out var e);

		if(r)
		{
			entity = new AutoId(e);
			return true;
		}
		else
		{ 
			entity = null;
			return false;
		}
	}

	public static AutoId Parse(string t)
	{
		return new AutoId(long.Parse(t));
	}

	public static AutoId Parse(ReadOnlySpan<char> t)
	{
		return new AutoId(long.Parse(t));
	}

	public override void Read(Reader reader)
	{
		I = reader.Read7BitEncodedInt64();
	}

	public override void Write(Writer writer)
	{
		writer.Write7BitEncodedInt64(I);
	}

	public override bool Equals(object obj)
	{
		return obj is AutoId id && Equals(id);
	}

	public override bool Equals(EntityId a)
	{
		return a is AutoId e && I == e.I;
	}

	public override int CompareTo(EntityId a)
	{
		return CompareTo((AutoId)a);
	}

	public int CompareTo(AutoId a)
	{
		return I.CompareTo(a.I);
	}

	public static bool operator == (AutoId l, AutoId r)
	{
		return l is null && r is null || l is not null && l.Equals((object)r); /// object cast is IMPORTANT!!
	}

	public static bool operator != (AutoId left, AutoId right)
	{
		return !(left == right);
	}
}

public class AutoIdJsonConverter : JsonConverter<AutoId>
{
	public override AutoId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return AutoId.Parse(reader.GetString());
	}

	public override void Write(Utf8JsonWriter writer, AutoId value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}

    public override void WriteAsPropertyName(Utf8JsonWriter writer, AutoId currency, JsonSerializerOptions options)
	{
		writer.WritePropertyName(currency.ToString());
	}

    public override AutoId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return Read(ref reader, typeToConvert, options);
	}
}
