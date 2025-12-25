using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class ProductFieldValueModel
{
	[JsonPropertyName("name")]
	public					Token Name { get; set; }

	[JsonPropertyName("type")]
	public					FieldType? Type { get; set; }

	[JsonPropertyName("value")]
	public					object Value { get; set; }
	
	[JsonPropertyName("children")]
	public IEnumerable<ProductFieldValueModel> Children { get; set; }
}

public class ProductFieldCompareModel
{
	[JsonPropertyName("from")]
	public					IEnumerable<ProductFieldValueModel> From { get; set; }

	[JsonPropertyName("to")]
	public					IEnumerable<ProductFieldValueModel> To { get; set; }
}