namespace Uccs.Fair;

public class SiteModel(Store site) : SiteBaseModel(site)
{
	public IEnumerable<string> ModeratorsIds { get; set; }
	public IEnumerable<string> AuthorsIds { get; set; }
}
