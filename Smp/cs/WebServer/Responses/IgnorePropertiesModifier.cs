using System.Text.Json.Serialization.Metadata;

namespace Uccs.Smp;

public class IgnorePropertiesModifier
{
	private static string ID_PROPERTY_NAME = "baseId";

	public static void IgnoreProperties(JsonTypeInfo typeInfo)
	{
		if (typeInfo.Type == typeof(ProductEntry))
		{
			foreach (JsonPropertyInfo propertyInfo in typeInfo.Properties)
			{
				if (propertyInfo.Name == ID_PROPERTY_NAME)
				{
					propertyInfo.ShouldSerialize = static (obj, value) => false;
				}
			}
		}
	}
}
