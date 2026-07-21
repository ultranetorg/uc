namespace Uccs.Fair;

public class UserPublicationModel
{
	public string Id { get; set; }

	public string StoreId { get; set; }
	public string StoreTitle { get; set; }

	public string CategoryId { get; set; }
	public string CategoryTitle { get; set; }

	public string ProductId { get; set; }
	public string ProductTitle { get; set; }

	//
	public string Url { get; set; }

	public UserPublicationModel(Publication publication, Store store, Category category, Product product)
	{
		Id = publication.Id.ToString();

		StoreId = store.Id.ToString();
		StoreTitle = store.Title;

		CategoryId = category.Id.ToString();
		CategoryTitle = category.Title;

		ProductId = product.Id.ToString();
		ProductTitle = PublicationUtils.GetTitle(publication, product);

		Url = PublicationUtils.GetUrl(publication);
	}
}
