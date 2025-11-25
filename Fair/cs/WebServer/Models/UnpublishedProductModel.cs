namespace Uccs.Fair;

public class UnpublishedProductModel(Product product, FairAccount account, AutoId productImageId)
{
	public string Id { get; } = product.Id.ToString();

	public ProductType Type { get; } = product.Type;

	public PublicationImageBaseModel Publication { get; } = new(product, productImageId);

	public AccountBaseAvatarModel Author { get; } = new(account);
}
