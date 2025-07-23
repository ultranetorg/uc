namespace Uccs.Fair;

public class UserDeletionModel(UserDeletion operation) : BaseVotableOperationModel(operation)
{
	public string UserId { get; set; } = operation.User.ToString();
}
