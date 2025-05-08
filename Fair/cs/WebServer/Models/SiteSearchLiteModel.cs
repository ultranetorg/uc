namespace Uccs.Fair;

public sealed class SiteSearchLiteModel
{
	public string Id { get; set; }

	public string Title { get; set; }

	public SiteSearchLiteModel(Site site)
	{
		Id = site.Id.ToString();
		Title = site.Title;
	}

	public SiteSearchLiteModel(string id, string title)
	{
		Id = id;
		Title = title;
	}
}
