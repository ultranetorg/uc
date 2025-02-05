namespace Uccs.Fair;

public class UserPublicationModel
{
	public string Id { get; set; }

	public string SiteId { get; set; }
	public string SiteTitle { get; set; }

	public string CategoryId { get; set; }
	public string CategoryTitle { get; set; }

	public string ProductId { get; set; }
	public string ProductTitle { get; set; }

	//
	public string Url { get; set; }

	public UserPublicationModel(Publication publication, Site site, Category category, Product product)
	{
		Id = publication.Id.ToString();

		SiteId = site.Id.ToString();
		SiteTitle = site.Title;

		CategoryId = category.Id.ToString();
		CategoryTitle = category.Title;

		ProductId = product.Id.ToString();
		ProductTitle = ProductUtils.GetTitle(product);

		Url = PublicationUtils.GetUrl(publication);
	}
}
