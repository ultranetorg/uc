namespace Uccs.Fair;

public class ProductFieldVersionReferenceModel
{
	public ProductFieldName Name { get; set; }

	public int Version { get; set; }

	public ProductFieldVersionReferenceModel(ProductFieldVersionReference reference)
	{
		Name = reference.Field;
		Version = reference.Version;
	}
}
