namespace Uccs.Fair;

public class ProductStoreModel
{
	public AutoId StoreId { get; init; }

	public AutoId PublicationId { get; init; }

	public string Title { get; init; }

	public AutoId? AvatarId { get; init; }
}
