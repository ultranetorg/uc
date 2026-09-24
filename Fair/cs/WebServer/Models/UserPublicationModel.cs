namespace Uccs.Fair;

public class UserPublicationModel(Publication publication, Store store, Category category, Product product)
{
	public AutoId Id { get; set; } = publication.Id;

	public AutoId StoreId { get; set; } = store.Id;
	public string StoreTitle { get; set; } = store.Title;

	public AutoId CategoryId { get; set; } = category.Id;
	public string CategoryTitle { get; set; } = category.Title;

	public AutoId ProductId { get; set; } = product.Id;
	public string ProductTitle { get; set; } = product.Title;

	//
	public string Url { get; set; } = PublicationUtils.GetUrl(publication);
}
