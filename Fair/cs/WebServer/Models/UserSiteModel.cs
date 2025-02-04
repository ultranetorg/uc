namespace Uccs.Fair; 

public class UserSiteModel : SiteBaseModel
{
	public int ProductsCount { get; set; }

	public string Url { get; set; }

	public UserSiteModel(Site site)
	{
		Id = site.Id.ToString();
		Type = site.Type;
		Title = site.Title;
	}
}
