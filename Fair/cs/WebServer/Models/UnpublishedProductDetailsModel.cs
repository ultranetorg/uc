namespace Uccs.Fair;

public class UnpublishedProductDetailsModel(Product product, FairUser account, AutoId? productImage) : UnpublishedProductModel(product, account, productImage)
{
	public string Title { get; } = PublicationUtils.GetLatestTitle(product);

	public string Description { get; } = PublicationUtils.GetLatestDescription(product);

	public string? LogoId { get; } = PublicationUtils.GetLatestLogo(product)?.ToString();

	public IEnumerable<ProductFieldValueModel>? Versions { get; set; }
}
