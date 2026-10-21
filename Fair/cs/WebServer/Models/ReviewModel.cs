namespace Uccs.Fair;

public class ReviewModel : BaseCommentModel
{
	public required byte Rating { get; init; }

	public required AutoId PublicationId { get; init; }
	public required string PublicationTitle { get; init; }
}
