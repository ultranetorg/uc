using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class TimeJsonConverter : JsonConverter<Time>
{
	public override bool CanConvert(Type typeToConvert)
	{
		return typeToConvert == typeof(Time);
	}

	public override Time Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, Time value, JsonSerializerOptions options)
	{
		writer.WriteNumberValue(value.Days);
	}
}
