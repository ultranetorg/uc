namespace Uccs.Fair;

public class UnpublishedPublicationModel
{
	public string Id { get; init; }

	public ProductType Type { get; init; }

	public string? Title { get; init; }
	public string? LogoId { get; init; }
	public int Updated { get; init; }

	public string AuthorId { get; init; }
	public string AuthorTitle { get; init; }
	public string? AuthorLogoId { get; init; }
}
