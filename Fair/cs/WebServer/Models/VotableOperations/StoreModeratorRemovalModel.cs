namespace Uccs.Fair;

public class StoreModeratorRemovalModel(StoreModeratorRemoval operation) : BaseVotableOperationModel(operation)
{
	public AutoId ModeratorId { get; set; } = operation.Moderator;
	public UserModel Moderator { get; init; } = null!;
}
