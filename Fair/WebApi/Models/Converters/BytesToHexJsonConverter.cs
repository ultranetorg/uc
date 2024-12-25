using System.Text.Json;
using Uccs;

namespace Explorer.Api.Models.Converters;

public class BytesToHexJsonConverter : JsonConverter<byte[]?>
{
	public override bool CanConvert(Type typeToConvert)
	{
		return typeToConvert == typeof(byte[]);
	}

	public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, byte[]? value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value?.ToHex());
	}
}
