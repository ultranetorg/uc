namespace Uccs.Fair;

public class ProductSearchResultBaseModel
{
	public string ProductId { get; init; }
	public string ProductTitle { get; init; }

	public string AuthorId { get; init; }
	public string AuthorTitle { get; init; }

	public string PublicationId { get; init; }

	public string? AvatarId { get; init; }
}