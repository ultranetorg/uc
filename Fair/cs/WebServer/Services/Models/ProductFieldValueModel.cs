namespace Uccs.Fair;

public class ProductFieldValueModel
{
	public					Token Name { get; set; }
	public					FieldType? Type { get; set; }
	public					object Value { get; set; }
	
	public IEnumerable<ProductFieldValueModel> Children { get; set; }
}

public class ProductFieldCompareModel
{
	public					IEnumerable<ProductFieldValueModel> From { get; set; }
	public					IEnumerable<ProductFieldValueModel> To { get; set; }
}