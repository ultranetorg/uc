namespace Uccs.Fair;

public class ProductFieldValueMetadataModel
{
	public					Token Name { get; set; }
	public					FieldType Type { get; set; }
	
}

public class ProductFieldValueModel
{
	public					Token Name { get; set; }
	public					FieldType? Type { get; set; }
	public					IEnumerable<ProductFieldValueMetadataModel>? Metadata { get; set; }
	public byte[]			Value { get; set; }
	
	public IEnumerable<ProductFieldValueModel> Children { get; set; }
}