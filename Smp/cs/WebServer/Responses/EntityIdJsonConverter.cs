using System.Text.Json.Serialization;
using System.Text.Json;

namespace Uccs.Smp;

public class EntityIdJsonConverter : JsonConverter<EntityId>
{
	public override bool CanConvert(Type typeToConvert)
	{
		return typeToConvert == typeof(EntityId);
	}

	public override EntityId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, EntityId value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
