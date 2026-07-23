namespace Uccs.Fair;

public class StoreModeratorRemovalModel(StoreModeratorRemoval operation) : BaseVotableOperationModel(operation)
{
	public string ModeratorId { get; set; } = operation.Moderator.ToString();
	public UserModel Moderator { get; init; } = null!;
}
