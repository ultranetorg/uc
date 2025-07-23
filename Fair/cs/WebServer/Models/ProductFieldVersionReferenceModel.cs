namespace Uccs.Fair;

public class ProductFieldVersionReferenceModel(ProductFieldVersionReference reference)
{
	public ProductFieldName Name { get; set; } = reference.Field;

	public int Version { get; set; } = reference.Version;
}
