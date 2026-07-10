namespace Uccs.Fair;

public class SiteModeratorRemovalModel(StoreModeratorRemoval operation) : BaseVotableOperationModel(operation)
{
	public string ModeratorId { get; set; } = operation.Moderator.ToString();
	public UserModel Moderator { get; init; } = null!;
}
