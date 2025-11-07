namespace Uccs.Fair;

public class PublicationBaseSiteModel(Product product, FairAccount account, byte[]? productImage)
{
	public string Id { get; } = product.Id.ToString();

	public PublicationImageBaseModel Publication { get; } = new PublicationImageBaseModel(product, productImage);

	public AccountBaseModel Author { get; } = new AccountBaseModel(account);
}
