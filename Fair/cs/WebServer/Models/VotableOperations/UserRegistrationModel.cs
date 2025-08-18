namespace Uccs.Fair;

public class UserRegistrationModel(UserRegistration operation) : BaseVotableOperationModel(operation)
{
	public string UserId { get; } = operation.Signer.Id.ToString();

	public string Address { get; } = operation.Signer.Address.ToString();
}
