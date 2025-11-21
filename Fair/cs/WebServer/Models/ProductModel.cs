namespace Uccs.Fair;

public class ProductModel(Product product)
{
	public string Id { get; } = product.Id.ToString();

	public ProductType Type { get; } = product.Type;

	public IEnumerable<ProductFieldValueModel>? Versions { get; set; }
}
