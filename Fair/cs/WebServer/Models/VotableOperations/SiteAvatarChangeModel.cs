namespace Uccs.Fair;

public class SiteAvatarChangeModel(SiteAvatarChange operation) : BaseVotableOperationModel(operation)
{
	public string FileId { get; set; } = operation.File.ToString();
}
