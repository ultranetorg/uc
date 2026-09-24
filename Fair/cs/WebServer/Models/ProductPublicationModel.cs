namespace Uccs.Fair;

public class ProductPublicationModel
{
	public AutoId PublicationId { get; init; }

	public AutoId StoreId { get; init; }
	public string StoreTitle { get; init; }

	public byte Rating { get; init; }
}
