namespace Uccs.Smp;

public class UserPublicationModel
{
	public string Id { get; set; }

	public string Title { get; set; }

	public string SiteId { get; set; }
	public string SiteTitle { get; set; }

	public string CategoryId { get; set; }
	public string CategoryTitle { get; set; }

	//
	public string Url { get; set; }

	public UserPublicationModel(Publication publication, string publicationTitle, Site site, string categoryTitle)
	{
		Id = publication.Id.ToString();
		Title = publicationTitle;
		SiteId = site.Id.ToString();
		SiteTitle = site.Title;
		CategoryId = publication.Category.ToString();
		CategoryTitle = categoryTitle;
	}
}
