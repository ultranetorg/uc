using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class LowerCaseEnumConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert)
	{
		return typeToConvert.IsEnum;
	}

	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var converterType = typeof(LowerCaseEnumWriteOnlyConverterInner<>).MakeGenericType(typeToConvert);
		return (JsonConverter) Activator.CreateInstance(converterType)!;
	}

	private class LowerCaseEnumWriteOnlyConverterInner<T> : JsonConverter<T> where T : struct, Enum
	{
		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString().ToLowerInvariant());
		}

		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			throw new NotSupportedException($"Deserialization of enum {typeof(T)} is not supported.");
		}
	}
}
