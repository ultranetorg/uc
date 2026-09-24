namespace Uccs.Fair;

public class UserUnregistrationModel(UserUnregistration operation) : BaseVotableOperationModel(operation)
{
	public AutoId UserId { get; set; } = operation.User;
}
