namespace Uccs.Fair; 

public class UserSiteModel(Site site) : SiteBaseModel(site)
{
	public int ProductsCount { get; init; }

	public string Url { get; init; }
}
