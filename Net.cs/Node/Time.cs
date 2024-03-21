using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Net
{
	public struct Time : IBinarySerializable
	{
		public int					Days;
		public const string			DateFormat = "yyyy-MM-dd";

		public static readonly Time	Zero = new Time(0);
		public static readonly Time	Empty = new Time(-1);
		public static DateTime		Start = new DateTime(2020, 1, 1);
		public static Time			Now(Clock clock) => new Time(clock.Now - Start);

		public static Time			operator-  (Time a, Time b) => new Time(a.Days - b.Days);
		public static Time			operator+  (Time a, Time b) => new Time(a.Days + b.Days);
		public static bool			operator<  (Time a, Time b) => a.Days < b.Days;
		public static bool			operator>  (Time a, Time b) => a.Days > b.Days;
		public static bool			operator<= (Time a, Time b) => a.Days <= b.Days;
		public static bool			operator>= (Time a, Time b) => a.Days >= b.Days;
		public static bool			operator== (Time a, Time b) => a.Days == b.Days;
		public static bool			operator!= (Time a, Time b) => a.Days != b.Days;

		public string				ToString(string format) => (Start + TimeSpan.FromDays(Days)).ToString(format);
		public static Time			Max(Time a, Time b) => a > b ? a : b;
		public TimeSpan				ToTimeSpan() => TimeSpan.FromDays(Days);

		public Time()
		{
		}

		public Time(int t)
		{
			Days = t;
		}

		public Time(TimeSpan time)
		{
			Days = (int)(time.Ticks/10_000_000/60/60/24);
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
			return o != null && o is Time t && t.Days == Days;
		}

		public override int GetHashCode()
		{
			return Days.GetHashCode();
		}

		public static Time FromYears(int years)
		{
			return new Time(365 * years);
		}

		public static Time FromDays(int days)
		{
			return new Time(days);
		}

		public void Read(BinaryReader r)
		{
			Days = r.Read7BitEncodedInt();
		}

		public void Write(BinaryWriter w)
		{
			w.Write7BitEncodedInt(Days);
		}
	}

	public class ChainTimeJsonConverter : JsonConverter<Time>
	{
		public override Time Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return new Time(reader.GetInt32());
		}

		public override void Write(Utf8JsonWriter writer, Time value, JsonSerializerOptions options)
		{
			writer.WriteNumberValue(value.Days);
		}
	}
}
