namespace Uccs.Fair;

public class UserRegistrationModel(UserRegistration operation) : BaseVotableOperationModel(operation)
{
	public string UserId { get; } = operation.User.Id.ToString();

	public string Address { get; } = operation.User.Owner.ToString();
}
