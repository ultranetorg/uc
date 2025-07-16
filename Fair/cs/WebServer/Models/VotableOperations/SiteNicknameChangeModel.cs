namespace Uccs.Fair;

public class SiteNicknameChangeModel(SiteNicknameChange operation) : BaseVotableOperationModel(operation)
{
	public string Nickname { get; set; } = operation.Nickname;
}
