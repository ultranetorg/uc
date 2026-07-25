namespace Uccs.Fair;

public class ProductSearchResultModel : ProductSearchResultBaseModel
{
	public IEnumerable<StorePublicationModel> StoresPublications { get; set; }
}