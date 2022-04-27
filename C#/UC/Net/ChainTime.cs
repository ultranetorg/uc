using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UC.Net
{
	public struct ChainTime
	{
		public long							Ticks;
		const int							Divider = 10_000;
		public const string					DateFormat = "yyyy-MM-dd H:mm:ss:fff";

		public static readonly ChainTime	Empty = new ChainTime(-1);

		public static ChainTime				operator-  (ChainTime a, ChainTime b) => new ChainTime(a.Ticks - b.Ticks);
		public static ChainTime				operator+  (ChainTime a, ChainTime b) => new ChainTime(a.Ticks + b.Ticks);
		public static bool					operator<  (ChainTime a, ChainTime b) => a.Ticks < b.Ticks;
		public static bool					operator>  (ChainTime a, ChainTime b) => a.Ticks > b.Ticks;
		public static bool					operator<= (ChainTime a, ChainTime b) => a.Ticks <= b.Ticks;
		public static bool					operator>= (ChainTime a, ChainTime b) => a.Ticks >= b.Ticks;
		public static bool					operator== (ChainTime a, ChainTime b) => a.Ticks == b.Ticks;
		public static bool					operator!= (ChainTime a, ChainTime b) => a.Ticks != b.Ticks;

		public string						ToString(string format) => Ticks >= 0 ? (new DateTime(Ticks * Divider)).ToString(format) : "~";

		public override string ToString()
		{
			return (new DateTime(Ticks * Divider)).ToString(DateFormat);
		}

		public override bool Equals(object o)
		{
			return o != null && o is ChainTime t && t.Ticks == Ticks;
		}

		public override int GetHashCode()
		{
			return Ticks.GetHashCode();
		}

		public static ChainTime FromYears(long years)
		{
			return new ChainTime(TimeSpan.FromDays(365).Ticks / Divider * years);
		}

		public ChainTime(long t)
		{
			Ticks = t;
		}
	}

	public class ChainTimeJsonConverter : JsonConverter<ChainTime>
	{
		public override ChainTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return new ChainTime(reader.GetInt64());
		}

		public override void Write(Utf8JsonWriter writer, ChainTime value, JsonSerializerOptions options)
		{
			writer.WriteNumberValue(value.Ticks);
		}
	}
}
