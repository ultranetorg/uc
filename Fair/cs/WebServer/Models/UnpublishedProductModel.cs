namespace Uccs.Fair;

public class UnpublishedProductModel(Product product, FairAccount account, byte[]? productImage)
{
	public string Id { get; } = product.Id.ToString();

	public ProductType Type { get; } = product.Type;

	public PublicationImageBaseModel Publication { get; } = new(product, productImage);

	public AccountBaseAvatarModel Author { get; } = new(account);
}
