namespace Uccs.Fair;

public class UserDeletionModel(UserUnregistration operation) : BaseVotableOperationModel(operation)
{
	public string UserId { get; set; } = operation.User.ToString();
}
