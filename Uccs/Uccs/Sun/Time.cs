using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net
{
	public struct Time : IBinarySerializable
	{
		public long					Ticks;
		const int					Divider = 10_000;
		public const string			DateFormat = "yyyy-MM-dd H:mm:ss:fff";

		public static readonly Time	Zero = new Time(0);
		public static readonly Time	Empty = new Time(-1);

		public static Time			operator-  (Time a, Time b) => new Time(a.Ticks - b.Ticks);
		public static Time			operator+  (Time a, Time b) => new Time(a.Ticks + b.Ticks);
		public static bool			operator<  (Time a, Time b) => a.Ticks < b.Ticks;
		public static bool			operator>  (Time a, Time b) => a.Ticks > b.Ticks;
		public static bool			operator<= (Time a, Time b) => a.Ticks <= b.Ticks;
		public static bool			operator>= (Time a, Time b) => a.Ticks >= b.Ticks;
		public static bool			operator== (Time a, Time b) => a.Ticks == b.Ticks;
		public static bool			operator!= (Time a, Time b) => a.Ticks != b.Ticks;

		public string				ToString(string format) => Ticks >= 0 ? (new DateTime(Ticks * Divider)).ToString(format) : "~";
		public static Time			Max(Time a, Time b) => a > b ? a : b;

		public Time()
		{
			Ticks = -1;
		}

		public Time(long t)
		{
			Ticks = t;
		}

		public override string ToString()
		{
			return Ticks == -1 ? "" : (new DateTime(Ticks * Divider)).ToString(DateFormat);
		}

		public static Time Parse(string v)
		{
			return new Time(DateTime.ParseExact(v, DateFormat, CultureInfo.InvariantCulture).Ticks / Divider);
		}

		public override bool Equals(object o)
		{
			return o != null && o is Time t && t.Ticks == Ticks;
		}

		public override int GetHashCode()
		{
			return Ticks.GetHashCode();
		}

		public static Time FromYears(int years)
		{
			return new Time(TicksFromYears(years));
		}

		public static Time FromDays(int daye)
		{
			return new Time(TimeSpan.FromDays(daye).Ticks / Divider);
		}

		public static long TicksFromYears(long years)
		{
			return TimeSpan.FromDays(365).Ticks / Divider * years;
		}

		public void Read(BinaryReader r)
		{
			Ticks = r.Read7BitEncodedInt64();
		}

		public void Write(BinaryWriter w)
		{
			w.Write7BitEncodedInt64(Ticks);
		}
	}

	public class ChainTimeJsonConverter : JsonConverter<Time>
	{
		public override Time Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return new Time(reader.GetInt64());
		}

		public override void Write(Utf8JsonWriter writer, Time value, JsonSerializerOptions options)
		{
			writer.WriteNumberValue(value.Ticks);
		}
	}
}
