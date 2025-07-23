namespace Uccs.Fair;

public class SiteAvatarChangeModel(SiteAvatarChange operation) : BaseVotableOperationModel(operation)
{
	public byte[] Image { get; set; } = operation.Image;
}
