namespace Uccs.Fair;

public class SiteModel(Site site, byte[]? avatar) : SiteBaseModel(site, avatar)
{
	//public IEnumerable<AccountBaseModel> Moderators { get; set; }

	public IEnumerable<SiteCategoryModel> Categories { get; set; }
}
