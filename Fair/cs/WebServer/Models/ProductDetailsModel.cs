namespace Uccs.Fair;

public class ProductDetailsModel(Product product, Author author, IEnumerable<FieldValueModel>? productFields)
{
	public string Id { get; } = product.Id.ToString();

	public ProductType ProductType { get; } = product.Type;

	public string? Title { get; } = PublicationUtils.GetLatestTitle(product);
	public string? Description { get; } = PublicationUtils.GetLatestDescription(product);
	public string? LogoFileId { get; } = PublicationUtils.GetLatestLogo(product)?.ToString();
	public int Updated { get; } = product.Updated.Hours;

	public string AuthorId { get; } = author.Id.ToString();
	public string AuthorTitle { get; } = author.Title;
	public string? AuthorFileId { get; } = author.Avatar?.ToString();

	public IEnumerable<FieldValueModel>? ProductFields { get; } = productFields;
}
