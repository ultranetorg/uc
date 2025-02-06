namespace Uccs.Fair;

public class SitePublicationModel
{
	public string Id { get; set; }

	public string CategoryId { get; set; }
	public string CategoryTitle { get; set; }

	public string ProductId { get; set; }
	public string ProductTitle { get; set; }

	public string AuthorId { get; set; }
	public string AuthorTitle { get; set; }

	public SitePublicationModel(EntityId id, Category category, Author author, Product product)
	{
		Id = id.ToString();

		CategoryId = category.Id.ToString();
		CategoryTitle = category.Title;

		ProductId = product.Id.ToString();
		ProductTitle = ProductUtils.GetTitle(product);

		AuthorId = author.Id.ToString();
		AuthorTitle = author.Title;
	}
}
