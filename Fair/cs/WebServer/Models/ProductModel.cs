namespace Uccs.Fair;

public class ProductModel(Product product)
{
	public string Id { get; } = product.Id.ToString();

	public ProductType Type { get; } = product.Type;

	public string Title { get; } = PublicationUtils.GetLatestTitle(product);

	public string Description { get; } = PublicationUtils.GetLatestDescription(product);

	public string? LogoId { get; } = PublicationUtils.GetLatestLogo(product)?.ToString();

	public IEnumerable<ProductFieldValueModel>? Versions { get; set; }
}
