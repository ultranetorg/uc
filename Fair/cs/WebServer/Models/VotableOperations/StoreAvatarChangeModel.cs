namespace Uccs.Fair;

public class StoreAvatarChangeModel(StoreAvatarChange operation) : BaseVotableOperationModel(operation)
{
	public AutoId FileId { get; set; } = operation.File;
}
