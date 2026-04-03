namespace Uccs.Fair;

public class UnpublishedProductDetailsModel(AutoId id, Product product, FairUser account, AutoId? productImage) : UnpublishedProductModel(id, product, account, productImage)
{
	public string Title { get; init; }

	public string Description { get; init; }

	public string? LogoId { get; init; }

	public IEnumerable<FieldValueModel>? Fields { get; init; }
}
