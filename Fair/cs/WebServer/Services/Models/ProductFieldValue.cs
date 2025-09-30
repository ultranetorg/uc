namespace Uccs.Fair;

public class ProductFieldValueMetadata
{
	public						Token Name { get; set; }
	public						FieldType Type { get; set; }
	
}

public class ProductFieldValue
{
	public						FieldType? Type { get; set; }
	public						IEnumerable<ProductFieldValueMetadata>? Metadata { get; set; }
	public byte[]				Value { get; set; }
	
	public IEnumerable<ProductFieldValue> Children { get; set; }
}