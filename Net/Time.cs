using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net;

public struct Time : IBinarySerializable
{
	public int						Seconds;
	public short					Days => (short)(Seconds/TimeSpan.SecondsPerDay);
	public short					Months => (short)(Seconds/(TimeSpan.SecondsPerDay * 30));
	public const string				DateFormat = "yyyy-MM-dd HH:mm:ss";

	public static readonly Time		Zero = new Time(0);
	public static readonly Time		Empty = new Time(-1);
	public static DateTime			Start = new DateTime(2026, 1, 1);

	public static Time				operator -  (Time a, Time b) => new Time(a.Seconds - b.Seconds);
	public static Time				operator +  (Time a, Time b) => new Time(a.Seconds + b.Seconds);
	public static bool				operator <  (Time a, Time b) => a.Seconds < b.Seconds;
	public static bool				operator >  (Time a, Time b) => a.Seconds > b.Seconds;
	public static bool				operator <= (Time a, Time b) => a.Seconds <= b.Seconds;
	public static bool				operator >= (Time a, Time b) => a.Seconds >= b.Seconds;
	public static bool				operator == (Time a, Time b) => a.Seconds == b.Seconds;
	public static bool				operator != (Time a, Time b) => a.Seconds != b.Seconds;

	public static Time				Now(IClock clock) => new Time(clock.Now - Start);
	public string					ToString(string format) => (Start + TimeSpan.FromSeconds(Seconds)).ToString(format);
	public TimeSpan					ToTimeSpan() => TimeSpan.FromSeconds(Seconds);
	public static Time				Max(Time a, Time b) => a > b ? a : b;

	public Time()
	{
	}

	public Time(int t)
	{
		Seconds = t;
	}
	
	public Time(TimeSpan time)
	{
		Seconds = (int)(time.Ticks/TimeSpan.TicksPerSecond);
	}

	public override string ToString()
	{
		return ToString(DateFormat);
	}

	public static Time Parse(string v)
	{
		return new Time(DateTime.ParseExact(v, DateFormat, CultureInfo.InvariantCulture) - Start);
	}

	public override bool Equals(object o)
	{
		return o != null && o is Time t && t.Seconds == Seconds;
	}

	public override int GetHashCode()
	{
		return Seconds.GetHashCode();
	}

	public static Time FromYears(int years)
	{
		return new Time(365 * 60 * 60 * 24 * years);
	}

	public static Time FromDays(int days)
	{
		return new Time(days * (int)TimeSpan.SecondsPerDay);
	}

	public void Read(BinaryReader r)
	{
		Seconds = r.Read7BitEncodedInt();
	}

	public void Write(BinaryWriter w)
	{
		w.Write7BitEncodedInt(Seconds);
	}
}

public class TimeJsonConverter : JsonConverter<Time>
{
	public override Time Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return new Time(reader.GetInt32());
	}

	public override void Write(Utf8JsonWriter writer, Time value, JsonSerializerOptions options)
	{
		writer.WriteNumberValue(value.Seconds);
	}
}
