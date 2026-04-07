namespace Uccs.Fair;

public class PublicationDetailsModel : ProductDetailsModel
{
	// NOTE: Fields can be null for unpublished publication. Unpublished publication have no category.
	public string? CategoryId { get; init; }
	public string? CategoryTitle { get; init; }

	// NOTE: Field can be null for unpublished publication. Unpublished publication have no rating.
	public int? Rating { get; init; }
}
