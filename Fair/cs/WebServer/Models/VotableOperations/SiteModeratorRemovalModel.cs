namespace Uccs.Fair;

public class SiteModeratorRemovalModel(SiteModeratorRemoval operation) : BaseVotableOperationModel(operation)
{
	public string ModeratorId { get; set; } = operation.Moderator.ToString();
}
