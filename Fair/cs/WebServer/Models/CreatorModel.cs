namespace Uccs.Fair;

public class CreatorModel
{
	public required AutoId Id { get; init; }
	public required string Title { get; init; }
	public required Role As { get; init; } = Role.User;
	public required AutoId? AvatarId { get; init; }
}
