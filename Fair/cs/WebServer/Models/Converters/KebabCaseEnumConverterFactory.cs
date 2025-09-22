using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Uccs.Fair;

public class KebabCaseEnumConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert)
	{
		return typeToConvert.IsEnum;
	}

	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var converterType = typeof(KebabCaseEnumWriteOnlyConverterInner<>).MakeGenericType(typeToConvert);
		return (JsonConverter) Activator.CreateInstance(converterType)!;
	}

	private class KebabCaseEnumWriteOnlyConverterInner<T> : JsonConverter<T> where T : struct, Enum
	{
		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		{
			var name = value.ToString();

			var kebab = Regex.Replace(name, "([a-z0-9])([A-Z])", "$1-$2")
							 .ToLowerInvariant();

			writer.WriteStringValue(kebab);
		}

		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			throw new NotSupportedException($"Deserialization of enum {typeof(T)} is not supported.");
		}
	}
}
