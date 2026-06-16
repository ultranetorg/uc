namespace Uccs.Fair;

public class UserUnregistrationModel(UserUnregistration operation) : BaseVotableOperationModel(operation)
{
	public string UserId { get; set; } = operation.User.ToString();
}
