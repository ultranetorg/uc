namespace Uccs.Fair;

public class UserRegistrationModel(UserRegistration operation) : BaseVotableOperationModel(operation)
{
	public AutoId UserId { get; } = operation.User.Id;

	public string Address { get; } = operation.User.Key.ToString();
}
