namespace Uccs.Fair;

public abstract class BaseCommentModel
{
	public required AutoId Id { get; init; }

	public required string Text { get; init; }

	public required CreatorModel Creator { get; init; }

	public required Time Created { get; init; }

}
