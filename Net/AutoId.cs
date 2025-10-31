using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net;

public class AutoId : EntityId
{
	public override int					B { get; set; }
	public int							E { get; set; }

	public static readonly AutoId		LastCreated = new AutoId {E = -1};
	public static readonly AutoId		God = new AutoId {E = -2};
	public static readonly AutoId		Father0 = new AutoId(165, 0);

	public AutoId()
	{
	}

	public AutoId(int b , int e)
	{
		B = b;
		E = e;
	}

	public override string ToString()
	{
		return $"{B}-{E}";
	}

	public override int GetHashCode()
	{
		return B;
	}

	public static bool TryParse(string t, out AutoId entity)
	{
		var i = t.IndexOf('-');

		entity = null;

		if(i == -1)
			return false;

		int e = 0;
		
		var r = int.TryParse(t.AsSpan(0, i), out var b) && int.TryParse(t.AsSpan(i + 1), out e);

		if(r)
		{
			if(b < 0 || b >= TableBase.BucketsCountMax)
				return false;

			if(e < 0)
				return false;

			entity = new AutoId(b, e);
		}

		return r;
	}

	public static AutoId Parse(string t)
	{
		var i = t.IndexOf('-');

		return new AutoId(int.Parse(t.AsSpan(0, i)), int.Parse(t.AsSpan(i + 1)));
	}

	public static AutoId Parse(ReadOnlySpan<char> t)
	{
		var i = t.IndexOf('-');

		return new AutoId(int.Parse(t.Slice(0, i)), int.Parse(t.Slice(i + 1)));
	}

	public override void Read(BinaryReader reader)
	{
		B	= reader.Read7BitEncodedInt();
		E	= reader.Read7BitEncodedInt();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write7BitEncodedInt(B);
		writer.Write7BitEncodedInt(E);
	}

	public override bool Equals(object obj)
	{
		return obj is AutoId id && Equals(id);
	}

	public override bool Equals(EntityId a)
	{
		return a is AutoId e && B == a.B && E == e.E;
	}

	public override int CompareTo(EntityId a)
	{
		return CompareTo((AutoId)a);
	}

	public int CompareTo(AutoId a)
	{
		var c = B.CompareTo(a.B);
		
		if(c != 0)
			return c;
		
		c = E.CompareTo(a.E);
		
		if(c != 0)
			return c;

		return 0;
	}

	public static bool operator == (AutoId left, AutoId right)
	{
		return left is null && right is null || left is not null && left.Equals((object)right); /// object cast is IMPORTANT!!
	}

	public static bool operator != (AutoId left, AutoId right)
	{
		return !(left == right);
	}
}

public class EntityIdJsonConverter : JsonConverter<AutoId>
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
