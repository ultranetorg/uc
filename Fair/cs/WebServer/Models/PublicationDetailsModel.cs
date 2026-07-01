namespace Uccs.Fair;

public class PublicationDetailsModel : ProductDetailsModel
{
	public string SiteId { get; init; }

	// NOTE: Fields can be null for unpublished publication. Unpublished publication have no category.
	public IEnumerable<CategoryPathItem>? Path { get; init; }

	// NOTE: Field can be null for unpublished publication. Unpublished publication have no rating.
	public int? Rating { get; init; }
}
