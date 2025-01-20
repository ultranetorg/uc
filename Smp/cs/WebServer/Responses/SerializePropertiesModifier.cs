using System.Text.Json.Serialization.Metadata;

namespace Uccs.Smp;

public class SerializePropertiesModifier
{
	private static string TYPE_PROPERTY_NAME = "Type";
	private static string VALUE_PROPERTY_NAME = "Value";

	public static void SerializeProperties(JsonTypeInfo typeInfo)
	{
		if (typeInfo.Type == typeof(ProductField))
		{
			foreach (JsonPropertyInfo propertyInfo in typeInfo.Properties)
			{
				if (propertyInfo.Name == TYPE_PROPERTY_NAME || propertyInfo.Name == VALUE_PROPERTY_NAME)
				{
					propertyInfo.ShouldSerialize = static (obj, value) => true;
				}
			}
		}
	}
}
