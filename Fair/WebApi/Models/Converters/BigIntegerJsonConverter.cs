using System.Text.Json;

namespace Explorer.Api.Models.Converters;

public class BigIntegerJsonConverter : JsonConverter<BigInteger>
{
	public override bool CanConvert(Type typeToConvert)
	{
		return typeToConvert == typeof(BigInteger);
	}

	public override BigInteger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, BigInteger value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
