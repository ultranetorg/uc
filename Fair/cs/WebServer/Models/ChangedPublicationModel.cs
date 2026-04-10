namespace Uccs.Fair;

// TODO: merge with ChangedPublicationModel and PublicationDetailsModel
public class ChangedPublicationModel
{
	public string Id { get; init; }

	public ProductType Type { get; init; }

	public string? Title { get; init; }
	public string? LogoId { get; init; }
	public int Updated { get; init; }

	public string AuthorId { get; init; }
	public string AuthorTitle { get; init; }
	public string? AuthorLogoId { get; init; }

	public string CategoryId { get; init; }
	public string CategoryTitle { get; init; }

	public int CurrentVersion { get; init; }
	public int LatestVersion { get; init; }
}