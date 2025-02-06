namespace Uccs.Fair;

public class SiteModel : SiteBaseModel
{
	public IEnumerable<AccountModel> Moderators { get; set; }

	public IEnumerable<CategoryBaseModel> Categories { get; set; }

	public SiteModel(Site site)
	{
		Id = site.Id.ToString();
		Title = site.Title;
	}
}
