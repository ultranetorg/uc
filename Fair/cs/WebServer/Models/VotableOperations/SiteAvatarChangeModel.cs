namespace Uccs.Fair;

public class SiteAvatarChangeModel(StoreAvatarChange operation) : BaseVotableOperationModel(operation)
{
	public string FileId { get; set; } = operation.File.ToString();
}
