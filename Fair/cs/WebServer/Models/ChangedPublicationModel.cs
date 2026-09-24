namespace Uccs.Fair;

// TODO: merge with ChangedPublicationModel and PublicationDetailsModel
public class ChangedPublicationModel
{
	public AutoId Id { get; init; }

	public ProductType Type { get; init; }

	public string? Title { get; init; }
	public AutoId? LogoId { get; init; }
	public Time Updated { get; init; }

	public AutoId AuthorId { get; init; }
	public string AuthorTitle { get; init; }
	public AutoId? AuthorLogoId { get; init; }

	public AutoId CategoryId { get; init; }
	public string CategoryTitle { get; init; }

	public int CurrentVersion { get; init; }
	public int LatestVersion { get; init; }
}