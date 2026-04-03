namespace Uccs.Fair;

public class UnpublishedProductModel(AutoId id, Product product, FairUser account, AutoId productImageId)
{
	public string Id { get; } = id.ToString();

	public ProductType Type { get; } = product.Type;

	public PublicationImageBaseModel Publication { get; } = new(product, productImageId);

	public AccountBaseAvatarModel Author { get; } = new(account);
}
