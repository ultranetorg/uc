namespace Uccs.Fair;

public class SiteNicknameChangeModel(SiteNameChange operation) : BaseVotableOperationModel(operation)
{
	public string Nickname { get; set; } = operation.Name;
}
