namespace Uccs.Fair;

public class ProductSearchResultModel
{
	public AutoId ProductId { get; init; }

	public AutoId? ProductLogoId { get; init; }
	public string ProductTitle { get; init; }
	public ProductType ProductType { get; init; }

	public string AuthorTitle { get; init; }

	public IEnumerable<ProductPublicationModel> Publications { get; set; }
	public bool HasMorePublications { get; init; }
}