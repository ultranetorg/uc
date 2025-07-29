namespace Uccs.Fair;

public class SiteAvatarChangeModel(SiteAvatarChange operation) : BaseVotableOperationModel(operation)
{
	public AutoId File { get; set; } = operation.File;
}
