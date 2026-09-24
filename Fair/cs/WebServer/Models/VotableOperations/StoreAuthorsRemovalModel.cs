namespace Uccs.Fair;

public class StoreAuthorsRemovalModel(StoreAuthorsRemoval operation) : BaseVotableOperationModel(operation)
{
	public IEnumerable<AutoId> RemovalsIds { get; set; } = operation.Authors;
	public IEnumerable<AuthorBaseAvatarModel> Removals { get; init; } = null!;
}
