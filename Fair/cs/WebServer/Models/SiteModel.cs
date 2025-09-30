namespace Uccs.Fair;

public class SiteModel(Site site, byte[]? avatar) : SiteBaseModel(site, avatar)
{
	public IEnumerable<SiteCategoryModel> Categories { get; set; }

	public IEnumerable<string> ModeratorsIds { get; set; }
	public IEnumerable<string> AuthorsIds { get; set; }
}
