namespace Uccs.Fair;

public class StoreAvatarChangeModel(StoreAvatarChange operation) : BaseVotableOperationModel(operation)
{
	public string FileId { get; set; } = operation.File.ToString();
}
