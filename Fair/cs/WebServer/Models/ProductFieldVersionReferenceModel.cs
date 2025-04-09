namespace Uccs.Fair;

public class ProductFieldVersionReferenceModel
{
	public string Name { get; set; }

	public int Version { get; set; }

	public ProductFieldVersionReferenceModel(ProductFieldVersionReference reference)
	{
		Name = reference.Name;
		Version = reference.Version;
	}
}
