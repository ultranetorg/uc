namespace Uccs.Fair;

public class SiteModel(Site site) : SiteBaseModel(site)
{
	//public IEnumerable<AccountBaseModel> Moderators { get; set; }

	public IEnumerable<CategoryBaseModel> Categories { get; set; }
}
