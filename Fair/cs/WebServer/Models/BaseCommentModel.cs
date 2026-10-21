namespace Uccs.Fair;

public abstract class BaseCommentModel
{
	public required AutoId Id { get; init; }

	public required string Text { get; init; }

	public required AutoId CreatorId { get; init; }
	public required string CreatorName { get; init; }
	public Role CreatorAs { get; init; } = Role.User;
	public required AutoId? CreatorAvatarId { get; init; }

	public required Time Created { get; init; }

}
