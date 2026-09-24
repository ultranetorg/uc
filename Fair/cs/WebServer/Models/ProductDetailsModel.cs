namespace Uccs.Fair;

public class ProductDetailsModel
{
	public AutoId Id { get; init; }

	public ProductType Type { get; init; }

	public string? Title { get; init; }
	public AutoId? LogoId { get; init; }
	public Time Updated { get; init; }

	public AutoId AuthorId { get; init; }
	public string AuthorTitle { get; init; }
	public AutoId? AuthorLogoId { get; init; }

	public IEnumerable<FieldValueModel>? Fields { get; init; }
}
