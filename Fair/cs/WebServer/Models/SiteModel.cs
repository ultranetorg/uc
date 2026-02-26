namespace Uccs.Fair;

public class SiteModel(Site site) : SiteBaseModel(site)
{
	public IEnumerable<SiteCategoryModel> Categories { get; set; }

	public IEnumerable<string> ModeratorsIds { get; set; }
	public IEnumerable<string> AuthorsIds { get; set; }
}
