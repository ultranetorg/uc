namespace Uccs.Fair;

public class ProductSearchResultModel : ProductSearchResultBaseModel
{
	public IEnumerable<ProductPublicationModel> Publications { get; set; }

	public bool HasMorePublications { get; set; }
}