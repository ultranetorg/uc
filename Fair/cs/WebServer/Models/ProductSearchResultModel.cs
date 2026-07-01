namespace Uccs.Fair;

public class ProductSearchResultModel : ProductSearchResultBaseModel
{
	public IEnumerable<SitePublicationModel> SitesPublications { get; set; }
}